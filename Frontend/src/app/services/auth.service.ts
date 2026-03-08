import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { tap } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private api = 'https://localhost:7061/api/auth';

  constructor(private http: HttpClient) {}

  register(data: any) {
    const identity = data?.email ?? data?.userName ?? data?.username ?? '';
    const fullName = data?.fullName ?? data?.FullName ?? '';
    const payload = {
      ...data,
      FullName: data?.FullName ?? fullName,
      fullName: fullName,
      email: data?.email ?? identity,
      userName: data?.userName ?? data?.username ?? identity,
      username: data?.username ?? data?.userName ?? identity
    };
    return this.http.post(`${this.api}/register`, payload);
  }

  login(data: any) {
    const identity = data?.identifier ?? data?.email ?? data?.userName ?? data?.username ?? '';
    const usernameFromEmail =
      typeof identity === 'string' && identity.includes('@') ? identity.split('@')[0] : identity;
    const payload = {
      ...data,
      email: data?.email ?? (String(identity).includes('@') ? identity : ''),
      userName: data?.userName ?? data?.username ?? usernameFromEmail,
      username: data?.username ?? data?.userName ?? usernameFromEmail
    };

    return this.http.post<any>(`${this.api}/login`, payload).pipe(
      tap(res => {
        const token = this.extractToken(res);

        const userId = res?.userId ?? res?.data?.userId ?? res?.user?.id ?? null;
        const role = res?.role ?? res?.data?.role ?? res?.user?.role ?? null;

        if (!token || token === 'undefined' || token === 'null') {
          throw new Error('Login response did not contain a valid token.');
        }

        localStorage.setItem('token', token);
        if (userId != null) localStorage.setItem('userId', String(userId));
        if (role != null) localStorage.setItem('role', String(role));
      })
    );
  }

  token() {
    const token = localStorage.getItem('token');
    if (!token || token === 'undefined' || token === 'null') return null;
    return token;
  }
  isLoggedIn() { return this.token() !== null; }
  role() { return localStorage.getItem('role'); }

  logout() {
    localStorage.clear();
  }

  private extractToken(payload: unknown): string | null {
    const byKnownKeys =
      (payload as any)?.token ??
      (payload as any)?.accessToken ??
      (payload as any)?.jwt ??
      (payload as any)?.Token ??
      (payload as any)?.AccessToken ??
      (payload as any)?.JWT ??
      (payload as any)?.data?.token ??
      (payload as any)?.data?.accessToken ??
      (payload as any)?.data?.jwt ??
      null;

    if (typeof byKnownKeys === 'string' && byKnownKeys.includes('.')) return byKnownKeys;

    const queue: unknown[] = [payload];
    while (queue.length) {
      const item = queue.shift();
      if (!item || typeof item !== 'object') continue;
      for (const [key, value] of Object.entries(item as Record<string, unknown>)) {
        if (typeof value === 'string') {
          const lowerKey = key.toLowerCase();
          const looksJwt = /^[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+$/.test(value);
          if (looksJwt || lowerKey.includes('token') || lowerKey.includes('jwt')) {
            return value;
          }
        } else if (value && typeof value === 'object') {
          queue.push(value);
        }
      }
    }
    return null;
  }
}
