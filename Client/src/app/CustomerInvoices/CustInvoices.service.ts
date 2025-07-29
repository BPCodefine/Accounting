import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CustInvoices } from './CustInvoices.model';
import { environment } from '@env/environment';

@Injectable({
  providedIn: 'root',
})
export class CustInvoiceService {

  constructor(private httpClient: HttpClient) {}

  getCustInvoices(fromDate: Date, toDate: Date = new Date(), Company: string): Observable<CustInvoices[]> {
    return this.httpClient.get<CustInvoices[]>(environment.apiUrl + '/api/CustomerInvoices' +
      `?FromDate=${fromDate.toISOString().split('T')[0]}&ToDate=${toDate.toISOString().split('T')[0]}&Company=${Company}`);
    }

  getLateCustInvoices(Company: string): Observable<CustInvoices[]> {
    return this.httpClient.get<CustInvoices[]>(environment.apiUrl + '/api/LateCustomerInvoices' + `?Company=${Company}`);
  }
}
