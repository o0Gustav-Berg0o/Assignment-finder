import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AdminUser } from './models';

@Injectable({ providedIn: 'root' })
export class AdminService {
  constructor(private readonly http: HttpClient) {}

  listUsers(): Observable<AdminUser[]> {
    return this.http.get<AdminUser[]>('/api/admin/users');
  }

  approve(id: string): Observable<void> {
    return this.http.post<void>(`/api/admin/users/${id}/approve`, { id });
  }

  reject(id: string): Observable<void> {
    return this.http.post<void>(`/api/admin/users/${id}/reject`, { id });
  }
}
