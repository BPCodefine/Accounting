import { Component } from '@angular/core';
import { RouterModule, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    MatToolbarModule,
    MatSidenavModule,
    MatIconModule,
    MatListModule,
    MatButtonModule,
    RouterModule
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'angular-sidenav';
  showFiller = false;

  menuItems: any[] = [
    { icon: 'home', label: 'Home', route: 'home' },
    { icon: 'assignment', label: 'InvExpenses', route: 'InvExpenses' },
    { icon: 'account_balance', label: 'BankAccountLedger', route: 'BankAccountLedger' },
    { icon: 'receipt_long', label: 'CustInvoices', route: 'CustomerInvoices' },
    { icon: 'description', label: 'VendorInvoices', route: 'VendorInvoices' }
  ];
}
