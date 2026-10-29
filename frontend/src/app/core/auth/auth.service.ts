import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, defer, finalize, firstValueFrom, from, map, of, shareReplay, tap, throwError } from 'rxjs';
import { API_BASE } from '../api.config';
import { AuthResponse, UserInfo } from './types/auth.types';

const REFRESH_TOKEN_KEY = 'comsci.refreshToken';
const REFRESH_LOCK_NAME = 'comsci.refresh';

// True only when the server answered that the refresh token is no good (so the person must sign in again).
// A network failure, a 5xx or a 429 says nothing about the token: it must be kept and tried again later.
export function isSessionRejected(error: unknown): boolean {
  return error instanceof HttpErrorResponse && error.status === 401;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  // The short-lived access token lives only in memory. The refresh token is kept in localStorage so a
  // page reload does not log the user out (the API issues tokens in the response body, not in cookies).
  private accessToken: string | null = null;
  private refreshRequest$: Observable<string> | null = null;

  private readonly currentUser = signal<UserInfo | null>(null);
  readonly user = this.currentUser.asReadonly();
  readonly isLoggedIn = computed(() => this.currentUser() !== null);
  readonly isAdmin = computed(() => this.currentUser()?.roles.includes('Admin') ?? false);

  getAccessToken(): string | null {
    return this.accessToken;
  }

  hasRefreshToken(): boolean {
    return this.readRefreshToken() !== null;
  }

  login(identifier: string, password: string): Observable<void> {
    return this.http.post<AuthResponse>(`${API_BASE}/auth/login`, { identifier, password }).pipe(
      tap((response) => this.applyResponse(response)),
      map(() => undefined),
    );
  }

  // Several requests can fail with 401 at the same moment; they all wait for one refresh call,
  // because every refresh token can be used only once.
  refreshAccessToken(): Observable<string> {
    if (this.refreshRequest$) {
      return this.refreshRequest$;
    }

    if (!this.readRefreshToken()) {
      return throwError(() => new Error('No refresh token'));
    }

    this.refreshRequest$ = this.runExclusively(this.requestNewTokens()).pipe(
      finalize(() => (this.refreshRequest$ = null)),
      shareReplay(1),
    );
    return this.refreshRequest$;
  }

  // Called once at start-up: turns a stored refresh token back into a signed-in session.
  restoreSession(): Promise<void> {
    if (!this.hasRefreshToken()) {
      return Promise.resolve();
    }

    return firstValueFrom(
      this.refreshAccessToken().pipe(
        catchError((error: unknown) => {
          // Only a rejected token ends the session. If the API is merely unreachable, keep the token for the next try.
          if (isSessionRejected(error)) {
            this.clearSession();
          }
          return of('');
        }),
      ),
    ).then(() => undefined);
  }

  logout(): void {
    const refreshToken = this.readRefreshToken();
    this.clearSession();

    if (refreshToken) {
      this.http.post(`${API_BASE}/auth/logout`, { refreshToken }).subscribe({ error: () => undefined });
    }
    this.router.navigate(['/login']);
  }

  // The refresh token is read when the request starts, which (with the lock) is after any other tab has finished:
  // sending a token that tab already replaced would look like a stolen token and sign the person out everywhere.
  private requestNewTokens(): Observable<string> {
    return defer(() => {
      const refreshToken = this.readRefreshToken();
      if (!refreshToken) {
        return throwError(() => new Error('No refresh token'));
      }

      return this.http.post<AuthResponse>(`${API_BASE}/auth/refresh`, { refreshToken }).pipe(
        tap((response) => this.applyResponse(response)),
        map((response) => response.accessToken),
      );
    });
  }

  // Tabs of the same browser share one refresh token, so only one of them may refresh at a time.
  // Browsers without the Web Locks API simply refresh at once.
  private runExclusively(task: Observable<string>): Observable<string> {
    const locks = typeof navigator === 'undefined' ? undefined : navigator.locks;
    if (!locks) {
      return task;
    }
    // request() hands back whatever the task returns; unwrap the promise it holds
    return from(locks.request(REFRESH_LOCK_NAME, () => firstValueFrom(task)).then((token) => token));
  }

  private applyResponse(response: AuthResponse): void {
    this.accessToken = response.accessToken;
    this.currentUser.set(response.user);
    localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
  }

  private clearSession(): void {
    this.accessToken = null;
    this.currentUser.set(null);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
  }

  private readRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }
}
