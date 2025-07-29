
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { VendorInvoices } from './VendorInvoices.model';
import { environment } from '@env/environment';

@Injectable({
  providedIn: 'root'
})
export class VendorInvoicesService {

  constructor(private httpClient: HttpClient) {}

  getVendorInvoices(fromDate: Date, toDate: Date = new Date(), Company: string): Observable<VendorInvoices[]> {
    return this.httpClient.get<VendorInvoices[]>(environment.apiUrl + '/api/VendorInvoices' +
      `?FromDate=${fromDate.toISOString().split('T')[0]}&ToDate=${toDate.toISOString().split('T')[0]}&Company=${Company}`);
    }

  getUnpaidVendorInvoices(fromDate: Date, toDate: Date = new Date(), Company: string): Observable<VendorInvoices[]> {
    return this.httpClient.get<VendorInvoices[]>(environment.apiUrl + '/api/VendorInvoices' +
      `?FromDate=${fromDate.toISOString().split('T')[0]}&ToDate=${toDate.toISOString().split('T')[0]}&Company=${Company}&OnlyOpen=true`);
    }
  }
