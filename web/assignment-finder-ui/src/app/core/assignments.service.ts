import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { withOptionalParams } from './http-params.util';
import { AssignmentsPage } from './models';

export interface AssignmentFilter {
  query?: string;
  area?: string;
  brokerId?: number;
  onlyInteresting?: boolean;
  page?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class AssignmentsService {
  constructor(private readonly http: HttpClient) {}

  list(filter: AssignmentFilter): Observable<AssignmentsPage> {
    const params = withOptionalParams(new HttpParams(), {
      query: filter.query,
      area: filter.area,
      brokerId: filter.brokerId,
      onlyInteresting: filter.onlyInteresting,
    })
      .set('page', filter.page ?? 1)
      .set('pageSize', filter.pageSize ?? 20);

    return this.http.get<AssignmentsPage>('/api/assignments', { params });
  }

  markInteresting(id: number): Observable<void> {
    return this.http.post<void>(`/api/assignments/${id}/interesting`, {});
  }

  unmarkInteresting(id: number): Observable<void> {
    return this.http.delete<void>(`/api/assignments/${id}/interesting`);
  }
}
