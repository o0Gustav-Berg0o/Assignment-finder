import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
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
    let params = new HttpParams();
    if (filter.query) {
      params = params.set('query', filter.query);
    }
    if (filter.area) {
      params = params.set('area', filter.area);
    }
    if (filter.brokerId) {
      params = params.set('brokerId', filter.brokerId);
    }
    if (filter.onlyInteresting) {
      params = params.set('onlyInteresting', filter.onlyInteresting);
    }
    params = params.set('page', filter.page ?? 1);
    params = params.set('pageSize', filter.pageSize ?? 20);

    return this.http.get<AssignmentsPage>('/api/assignments', { params });
  }

  markInteresting(id: number): Observable<void> {
    return this.http.post<void>(`/api/assignments/${id}/interesting`, {});
  }

  unmarkInteresting(id: number): Observable<void> {
    return this.http.delete<void>(`/api/assignments/${id}/interesting`);
  }
}
