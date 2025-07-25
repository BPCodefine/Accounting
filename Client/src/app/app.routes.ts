import { Routes } from '@angular/router';
import { HomeComponent } from './Home/home.component';
import { InvExpensesComponent } from './InvExpenses/InvExpenses.component';
import { BankAccountLedgerComponent } from './BankAccountLedger/BankAccountLedger.component';
import { CustomerInvoicesComponent } from './CustomerInvoices/CustomerInvoices';

export const routes: Routes = [
    { path: '', pathMatch: 'full', redirectTo: 'home' },
    { path: 'home', component: HomeComponent },
    { path: 'InvExpenses', component: InvExpensesComponent },
    { path: 'BankAccountLedger', component: BankAccountLedgerComponent},
    { path: 'CustomerInvoices', component: CustomerInvoicesComponent}
]
