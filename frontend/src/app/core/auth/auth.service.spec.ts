import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { installMemoryLocalStorage } from '../../../testing/memory-storage';
import { AuthResponse } from '../models/auth.models';
import { AuthService } from './auth.service';

const adminResponse: AuthResponse = {
  accessToken: 'access-1',
  expiresAt: '2030-01-01T00:00:00Z',
  refreshToken: 'refresh-1',
  user: { id: '1', fullName: 'Admin', email: 'admin@example.com', roles: ['Admin'] },
};

describe('AuthService', () => {
  let auth: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    installMemoryLocalStorage();
    TestBed.configureTestingModule({
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    auth = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('starts signed out', () => {
    expect(auth.isLoggedIn()).toBe(false);
    expect(auth.isAdmin()).toBe(false);
  });

  it('login keeps the user, the access token and the refresh token', () => {
    auth.login('admin@example.com', 'secret').subscribe();
    http.expectOne('/api/auth/login').flush(adminResponse);

    expect(auth.isLoggedIn()).toBe(true);
    expect(auth.isAdmin()).toBe(true);
    expect(auth.getAccessToken()).toBe('access-1');
    expect(localStorage.getItem('comsci.refreshToken')).toBe('refresh-1');
  });

  it('shares one refresh call between simultaneous callers', () => {
    localStorage.setItem('comsci.refreshToken', 'refresh-0');

    const tokens: string[] = [];
    auth.refreshAccessToken().subscribe((token) => tokens.push(token));
    auth.refreshAccessToken().subscribe((token) => tokens.push(token));

    http.expectOne('/api/auth/refresh').flush(adminResponse);

    expect(tokens).toEqual(['access-1', 'access-1']);
  });

  it('restoreSession clears a refresh token the server no longer accepts', async () => {
    localStorage.setItem('comsci.refreshToken', 'stale');

    const restored = auth.restoreSession();
    http.expectOne('/api/auth/refresh').flush({}, { status: 401, statusText: 'Unauthorized' });
    await restored;

    expect(auth.isLoggedIn()).toBe(false);
    expect(localStorage.getItem('comsci.refreshToken')).toBeNull();
  });
});
