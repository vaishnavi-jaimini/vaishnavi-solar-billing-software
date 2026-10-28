import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ProformaInvoice, ProformaInvoiceUpsert } from '../models/models';

@Injectable({ providedIn: 'root' })
export class ProformaInvoiceService {
  private readonly base = `${environment.apiUrl}/proformainvoices`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ProformaInvoice[]> {
    return this.http.get<ProformaInvoice[]>(this.base);
  }

  getById(id: number): Observable<ProformaInvoice> {
    return this.http.get<ProformaInvoice>(`${this.base}/${id}`);
  }

  create(dto: ProformaInvoiceUpsert): Observable<ProformaInvoice> {
    return this.http.post<ProformaInvoice>(this.base, dto);
  }

  update(id: number, dto: ProformaInvoiceUpsert): Observable<ProformaInvoice> {
    return this.http.put<ProformaInvoice>(`${this.base}/${id}`, dto);
  }

  convertToInvoice(id: number): Observable<{ invoiceId: number; invoiceNumber: string }> {
    return this.http.post<{ invoiceId: number; invoiceNumber: string }>(`${this.base}/${id}/convert-to-invoice`, {});
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
