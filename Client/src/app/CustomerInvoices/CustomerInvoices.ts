import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DxDateRangeBoxModule, DxDataGridModule } from 'devextreme-angular';
import { DxDateRangeBoxTypes } from "devextreme-angular/ui/date-range-box"

import { CustInvoices } from './CustInvoices.model';
import { CustInvoiceService } from './CustInvoices.service';

@Component({
  selector: 'app-customer-invoices',
  imports: [CommonModule,
            DxDataGridModule,
            DxDateRangeBoxModule],
  templateUrl: './CustomerInvoices.html',
  styleUrl: './CustomerInvoices.css'
})
export class CustomerInvoicesComponent {
  loading: boolean = true;

  minDate: Date = new Date(2020, 7, 1);
  startDate: Date = new Date(new Date().setFullYear(new Date().getFullYear() - 1));
  endDate: Date = new Date();
  currentValue: [Date, Date] = [this.startDate, this.endDate];

  invoices: CustInvoices[] = [];

  constructor(private custInvoicesService: CustInvoiceService, private cdr: ChangeDetectorRef) {}

  onCurrentValueChanged(e: DxDateRangeBoxTypes.ValueChangedEvent) {
    this.startDate = e.value[0];
    this.endDate = e.value[1];
    this.FetchInvoices();
    //console.log('Current value changed:', this.currentValue);
  }

  FetchInvoices() {
    this.loading = true;
    this.custInvoicesService.getCustInvoices(this.startDate, this.endDate, "cdf").subscribe({
      next: (data) => {
        this.invoices = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error fetching invoices:', error);
        this.loading = false;
      },
    });
  }
}
