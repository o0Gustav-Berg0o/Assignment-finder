import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Broker, UpsertBroker } from './models';

@Injectable({ providedIn: 'root' })
export class BrokersService {
  constructor(private readonly http: HttpClient) {}

  list(): Observable<Broker[]> {
    return this.http.get<Broker[]>('/api/brokers');
  }

  create(broker: UpsertBroker): Observable<Broker> {
    return this.http.post<Broker>('/api/brokers', broker);
  }

  update(id: number, broker: UpsertBroker): Observable<Broker> {
    return this.http.put<Broker>(`/api/brokers/${id}`, { id, ...broker });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`/api/brokers/${id}`);
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

  scanAll(): Observable<{ brokersScanned: number; newAssignments: number; newMatches: number }> {
    return this.http.post<{ brokersScanned: number; newAssignments: number; newMatches: number }>(
      '/api/brokers/scan-all',
      {},
    );
  }

  revealPassword(id: number): Observable<{ password: string }> {
    return this.http.get<{ password: string }>(`/api/brokers/${id}/login-password`);
  }
}
