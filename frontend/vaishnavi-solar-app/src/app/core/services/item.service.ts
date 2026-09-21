import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Item, ItemUpsert, StockAdjustment } from '../models/models';

@Injectable({ providedIn: 'root' })
export class ItemService {
  private readonly base = `${environment.apiUrl}/items`;
  private readonly stockBase = `${environment.apiUrl}/stock`;

  constructor(private http: HttpClient) {}

  getAll(search?: string, activeOnly = false): Observable<Item[]> {
    const params: Record<string, string> = {};
    if (search) params['search'] = search;
    if (activeOnly) params['activeOnly'] = 'true';
    return this.http.get<Item[]>(this.base, { params });
  }

  getById(id: number): Observable<Item> {
    return this.http.get<Item>(`${this.base}/${id}`);
  }

  create(dto: ItemUpsert): Observable<Item> {
    return this.http.post<Item>(this.base, dto);
  }

  update(id: number, dto: ItemUpsert): Observable<Item> {
    return this.http.put<Item>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.base}/${id}`);
  }

  adjustStock(dto: StockAdjustment): Observable<any> {
    return this.http.post(`${this.stockBase}/adjust`, dto);
  }

  getStockTransactions(itemId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.stockBase}/transactions/${itemId}`);
  }
}
