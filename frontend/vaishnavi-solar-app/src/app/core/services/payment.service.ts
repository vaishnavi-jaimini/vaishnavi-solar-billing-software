import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Payment, PaymentUpsert } from '../models/models';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly base = `${environment.apiUrl}/payments`;

  constructor(private http: HttpClient) {}

  getAll(invoiceId?: number): Observable<Payment[]> {
    const params: Record<string, string> = {};
    if (invoiceId) params['invoiceId'] = invoiceId.toString();
    return this.http.get<Payment[]>(this.base, { params });
  }

  create(dto: PaymentUpsert): Observable<Payment> {
    return this.http.post<Payment>(this.base, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
