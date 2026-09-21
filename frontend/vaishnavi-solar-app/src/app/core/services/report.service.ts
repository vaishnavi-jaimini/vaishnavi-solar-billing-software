import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Dashboard, GstSummary, OutstandingDueRow, SalesReport, StockReportRow } from '../models/models';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly base = `${environment.apiUrl}/reports`;

  constructor(private http: HttpClient) {}

  sales(from?: string, to?: string): Observable<SalesReport> {
    const params: Record<string, string> = {};
    if (from) params['from'] = from;
    if (to) params['to'] = to;
    return this.http.get<SalesReport>(`${this.base}/sales`, { params });
  }

  gstSummary(from?: string, to?: string): Observable<GstSummary> {
    const params: Record<string, string> = {};
    if (from) params['from'] = from;
    if (to) params['to'] = to;
    return this.http.get<GstSummary>(`${this.base}/gst-summary`, { params });
  }

  outstandingDues(): Observable<OutstandingDueRow[]> {
    return this.http.get<OutstandingDueRow[]>(`${this.base}/outstanding-dues`);
  }

  stock(lowOnly = false): Observable<StockReportRow[]> {
    const params: Record<string, string> = {};
    if (lowOnly) params['lowOnly'] = 'true';
    return this.http.get<StockReportRow[]>(`${this.base}/stock`, { params });
  }

  dashboard(): Observable<Dashboard> {
    return this.http.get<Dashboard>(`${this.base}/dashboard`);
  }
}
