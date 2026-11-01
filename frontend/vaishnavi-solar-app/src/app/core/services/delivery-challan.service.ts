import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DeliveryChallan, DeliveryChallanUpsert } from '../models/models';

@Injectable({ providedIn: 'root' })
export class DeliveryChallanService {
  private readonly base = `${environment.apiUrl}/deliverychallans`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<DeliveryChallan[]> {
    return this.http.get<DeliveryChallan[]>(this.base);
  }

  getById(id: number): Observable<DeliveryChallan> {
    return this.http.get<DeliveryChallan>(`${this.base}/${id}`);
  }

  create(dto: DeliveryChallanUpsert): Observable<DeliveryChallan> {
    return this.http.post<DeliveryChallan>(this.base, dto);
  }

  update(id: number, dto: DeliveryChallanUpsert): Observable<DeliveryChallan> {
    return this.http.put<DeliveryChallan>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
