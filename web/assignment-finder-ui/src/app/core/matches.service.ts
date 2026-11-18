import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { MatchesPage } from './models';

@Injectable({ providedIn: 'root' })
export class MatchesService {
  constructor(private readonly http: HttpClient) {}

  list(unseenOnly: boolean, page = 1, pageSize = 20): Observable<MatchesPage> {
    return this.http.get<MatchesPage>('/api/matches', {
      params: { unseenOnly, page, pageSize },
    });
  }

  markSeen(id: number): Observable<void> {
    return this.http.post<void>(`/api/matches/${id}/seen`, { id });
  }
}
