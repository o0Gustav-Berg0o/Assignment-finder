import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';

interface LoginResponse {
  token: string;
}

const TOKEN_KEY = 'assignment-finder.token';
const ROLE_CLAIM_KEYS = ['role', 'roles', 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

function decodeRoles(token: string): string[] {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    for (const key of ROLE_CLAIM_KEYS) {
      const value = payload[key];
      if (!value) {
        continue;
      }
      return Array.isArray(value) ? value : [value];
    }
  } catch {
    // malformed/unexpected token shape - treat as no roles
  }
  return [];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenSignal = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  readonly isLoggedIn = computed(() => this.tokenSignal() !== null);
  readonly isAdmin = computed(() => {
    const token = this.tokenSignal();
    return token !== null && decodeRoles(token).includes('Admin');
  });

  constructor(private readonly http: HttpClient) {}

  get token(): string | null {
    return this.tokenSignal();
  }

  register(email: string, password: string): Observable<{ isApproved: boolean }> {
    return this.http.post<{ isApproved: boolean }>('/api/auth/register', { email, password });
  }

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>('/api/auth/login', { email, password }).pipe(
      tap((res) => {
        localStorage.setItem(TOKEN_KEY, res.token);
        this.tokenSignal.set(res.token);
      }),
    );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    this.tokenSignal.set(null);
  }
}
