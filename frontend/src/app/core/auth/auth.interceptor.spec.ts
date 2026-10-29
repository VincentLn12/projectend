import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { installMemoryLocalStorage } from '../../../testing/memory-storage';
import { AuthResponse } from './types/auth.types';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';

const loginResponse = (accessToken: string, refreshToken: string): AuthResponse => ({
  accessToken,
  expiresAt: '2030-01-01T00:00:00Z',
  refreshToken,
  user: { id: '1', fullName: 'Admin', email: 'admin@example.com', roles: ['Admin'] },
});

describe('authInterceptor', () => {
  let client: HttpClient;
  let auth: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    installMemoryLocalStorage();
    // Without the Web Locks API a refresh starts at once, which keeps these tests simple; the lock itself has its own test.
    vi.stubGlobal('navigator', {});
    TestBed.configureTestingModule({
      providers: [provideRouter([{ path: '**', children: [] }]), provideHttpClient(withInterceptors([authInterceptor])), provideHttpClientTesting()],
    });
    client = TestBed.inject(HttpClient);
    auth = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);

    auth.login('admin@example.com', 'secret').subscribe();
    http.expectOne('/api/auth/login').flush(loginResponse('access-1', 'refresh-1'));
  });

  afterEach(() => {
    http.verify();
    vi.unstubAllGlobals();
  });

  it('sends the access token to the API', () => {
    client.get('/api/me').subscribe();

    const request = http.expectOne('/api/me');
    expect(request.request.headers.get('Authorization')).toBe('Bearer access-1');
    request.flush({});
  });

  it('does not send the token to other sites', () => {
    client.get('https://example.com/data').subscribe();

    const request = http.expectOne('https://example.com/data');
    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush({});
  });

  it('on 401 refreshes once and repeats the request with the new token', () => {
    let result: unknown;
    client.get('/api/admin/news').subscribe((value) => (result = value));

    http.expectOne('/api/admin/news').flush({}, { status: 401, statusText: 'Unauthorized' });
    http.expectOne('/api/auth/refresh').flush(loginResponse('access-2', 'refresh-2'));

    const retry = http.expectOne('/api/admin/news');
    expect(retry.request.headers.get('Authorization')).toBe('Bearer access-2');
    retry.flush({ ok: true });

    expect(result).toEqual({ ok: true });
  });
  it('stays signed in when the refresh call fails only because the API is unreachable', () => {
    let failed = false;
    client.get('/api/admin/news').subscribe({ error: () => (failed = true) });

    http.expectOne('/api/admin/news').flush({}, { status: 401, statusText: 'Unauthorized' });
    http.expectOne('/api/auth/refresh').flush({}, { status: 503, statusText: 'Service Unavailable' });

    expect(failed).toBe(true);
    expect(auth.isLoggedIn()).toBe(true);
    expect(localStorage.getItem('comsci.refreshToken')).toBe('refresh-1');
  });

  it('signs out when the server refuses the refresh token', () => {
    client.get('/api/admin/news').subscribe({ error: () => undefined });

    http.expectOne('/api/admin/news').flush({}, { status: 401, statusText: 'Unauthorized' });
    http.expectOne('/api/auth/refresh').flush({}, { status: 401, statusText: 'Unauthorized' });
    http.expectOne('/api/auth/logout').flush(null);

    expect(auth.isLoggedIn()).toBe(false);
    expect(localStorage.getItem('comsci.refreshToken')).toBeNull();
  });
});
