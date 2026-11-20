import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { withOptionalParams } from './http-params.util';
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
    const params = withOptionalParams(new HttpParams(), {
      query: filter.query,
      area: filter.area,
      unseenOnly: filter.unseenOnly,
    })
      .set('page', filter.page ?? 1)
      .set('pageSize', filter.pageSize ?? 20);

    return this.http.get<MatchesPage>('/api/matches', { params });
  }

  markSeen(id: number): Observable<void> {
    return this.http.post<void>(`/api/matches/${id}/seen`, { id });
  }
}
