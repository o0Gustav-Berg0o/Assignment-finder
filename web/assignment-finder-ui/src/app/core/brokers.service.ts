import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Broker } from './models';

@Injectable({ providedIn: 'root' })
export class BrokersService {
  constructor(private readonly http: HttpClient) {}

  list(): Observable<Broker[]> {
    return this.http.get<Broker[]>('/api/brokers');
  }

  setEnabled(id: number, isEnabled: boolean): Observable<void> {
    return this.http.post<void>(`/api/brokers/${id}/enabled`, { id, isEnabled });
  }

  scanNow(id: number): Observable<{ newAssignments: number; newMatches: number }> {
    return this.http.post<{ newAssignments: number; newMatches: number }>(
      `/api/brokers/${id}/scan-now`,
      { id },
    );
  }
}
