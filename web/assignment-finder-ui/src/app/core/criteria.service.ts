import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Criterion } from './models';

type UpsertCriterion = Omit<Criterion, 'id'>;

@Injectable({ providedIn: 'root' })
export class CriteriaService {
  constructor(private readonly http: HttpClient) {}

  list(): Observable<Criterion[]> {
    return this.http.get<Criterion[]>('/api/criteria');
  }

  create(criterion: UpsertCriterion): Observable<Criterion> {
    return this.http.post<Criterion>('/api/criteria', criterion);
  }

  update(id: number, criterion: UpsertCriterion): Observable<Criterion> {
    return this.http.put<Criterion>(`/api/criteria/${id}`, { id, ...criterion });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`/api/criteria/${id}`);
  }
}
