import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Customer, CustomerUpsert } from '../models/models';

@Injectable({ providedIn: 'root' })
export class CustomerService {
  private readonly base = `${environment.apiUrl}/customers`;

  constructor(private http: HttpClient) {}

  getAll(search?: string): Observable<Customer[]> {
    const params: Record<string, string> = {};
    if (search) params['search'] = search;
    return this.http.get<Customer[]>(this.base, { params });
  }

  getById(id: number): Observable<Customer> {
    return this.http.get<Customer>(`${this.base}/${id}`);
  }

  create(dto: CustomerUpsert): Observable<Customer> {
    return this.http.post<Customer>(this.base, dto);
  }

  update(id: number, dto: CustomerUpsert): Observable<Customer> {
    return this.http.put<Customer>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
