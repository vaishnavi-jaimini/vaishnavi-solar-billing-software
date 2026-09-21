import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Company } from '../models/models';

@Injectable({ providedIn: 'root' })
export class CompanyService {
  private readonly base = `${environment.apiUrl}/company`;

  constructor(private http: HttpClient) {}

  get(): Observable<Company> {
    return this.http.get<Company>(this.base);
  }

  update(company: Company): Observable<Company> {
    return this.http.put<Company>(this.base, company);
  }
}
