import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { MatchesPage } from './models';

export interface MatchFilter {
  query?: string;
  area?: string;
  unseenOnly?: boolean;
  page?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class MatchesService {
  constructor(private readonly http: HttpClient) {}

  list(filter: MatchFilter): Observable<MatchesPage> {
    let params = new HttpParams();
    if (filter.query) {
      params = params.set('query', filter.query);
    }
    if (filter.area) {
      params = params.set('area', filter.area);
    }
    if (filter.unseenOnly) {
      params = params.set('unseenOnly', filter.unseenOnly);
    }
    params = params.set('page', filter.page ?? 1);
    params = params.set('pageSize', filter.pageSize ?? 20);

    return this.http.get<MatchesPage>('/api/matches', { params });
  }

  markSeen(id: number): Observable<void> {
    return this.http.post<void>(`/api/matches/${id}/seen`, { id });
  }
}
