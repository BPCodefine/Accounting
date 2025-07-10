import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BankAccountLedgerModel } from './BankAccountLedger.model';
import { environment } from '@env/environment';

@Injectable({
  providedIn: 'root'
})
export class BankAccountLedgerService {

  private apiUrl = environment.apiUrl + '/api/BankAccountLedgerItems';

  constructor(private httpClient: HttpClient) {}

  getLedgerEntries(toDate: Date, fromAccList: string[]): Observable<BankAccountLedgerModel[]> {
    return this.httpClient.get<BankAccountLedgerModel[]>(this.apiUrl + `?toDate=${toDate.toISOString().split('T')[0]}&ExtraAccList=${encodeURIComponent(fromAccList.join(','))}`);
  }

}
