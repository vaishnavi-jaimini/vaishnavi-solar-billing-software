import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Invoice, InvoiceUpsert } from '../models/models';
import { InvoiceStatus } from '../models/enums';

@Injectable({ providedIn: 'root' })
export class InvoiceService {
  private readonly base = `${environment.apiUrl}/invoices`;

  constructor(private http: HttpClient) {}

  getAll(filters?: { status?: InvoiceStatus; from?: string; to?: string }): Observable<Invoice[]> {
    const params: Record<string, string> = {};
    if (filters?.status) params['status'] = filters.status;
    if (filters?.from) params['from'] = filters.from;
    if (filters?.to) params['to'] = filters.to;
    return this.http.get<Invoice[]>(this.base, { params });
  }

  getById(id: number): Observable<Invoice> {
    return this.http.get<Invoice>(`${this.base}/${id}`);
  }

  create(dto: InvoiceUpsert): Observable<Invoice> {
    return this.http.post<Invoice>(this.base, dto);
  }

  convertQuotation(quotationId: number): Observable<Invoice> {
    return this.http.post<Invoice>(`${this.base}/convert-quotation/${quotationId}`, {});
  }
}
