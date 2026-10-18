import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Quotation, QuotationUpsert } from '../models/models';
import { QuotationStatus } from '../models/enums';

@Injectable({ providedIn: 'root' })
export class QuotationService {
  private readonly base = `${environment.apiUrl}/quotations`;

  constructor(private http: HttpClient) {}

  getAll(status?: QuotationStatus): Observable<Quotation[]> {
    const params: Record<string, string> = {};
    if (status) params['status'] = status;
    return this.http.get<Quotation[]>(this.base, { params });
  }

  getById(id: number): Observable<Quotation> {
    return this.http.get<Quotation>(`${this.base}/${id}`);
  }

  create(dto: QuotationUpsert): Observable<Quotation> {
    return this.http.post<Quotation>(this.base, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
