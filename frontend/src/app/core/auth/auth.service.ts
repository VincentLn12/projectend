import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, finalize, firstValueFrom, map, of, shareReplay, tap, throwError } from 'rxjs';
import { API_BASE } from '../api.config';
import { AuthResponse, UserInfo } from '../models/auth.models';

const REFRESH_TOKEN_KEY = 'comsci.refreshToken';

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

    const refreshToken = this.readRefreshToken();
    if (!refreshToken) {
      return throwError(() => new Error('No refresh token'));
    }

    this.refreshRequest$ = this.http.post<AuthResponse>(`${API_BASE}/auth/refresh`, { refreshToken }).pipe(
      tap((response) => this.applyResponse(response)),
      map((response) => response.accessToken),
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
        catchError(() => {
          this.clearSession();
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
