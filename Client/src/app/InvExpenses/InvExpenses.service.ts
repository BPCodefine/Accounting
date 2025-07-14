import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { InvExpensesModel } from './InvExpenses.model';
import { environment } from '@env/environment';

@Injectable({
  providedIn: 'root',
})
export class InvExpensesService {
  private apiUrl = environment.apiUrl + '/api/PurchInvAndExpenses';

  constructor(private httpClient: HttpClient) {}

  getInvoices(fromDate: Date, toDate: Date = new Date()): Observable<InvExpensesModel[]> {
    return this.httpClient.get<InvExpensesModel[]>(this.apiUrl +
      `/betweenDates?DateFrom=${fromDate.toISOString().split('T')[0]}&DateTo=${toDate.toISOString().split('T')[0]}`);
    }

  getAllInvoices(): Observable<InvExpensesModel[]> {
    return this.httpClient.get<InvExpensesModel[]>(this.apiUrl);
  }
}
