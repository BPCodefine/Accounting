import { Component, OnInit, HostListener, ChangeDetectorRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DxDateRangeBoxModule, DxCheckBoxModule, DxDataGridModule } from 'devextreme-angular';
import { DxDateRangeBoxTypes } from "devextreme-angular/ui/date-range-box"

import { VendorInvoices } from './VendorInvoices.model';
import { VendorInvoicesService } from './VendorInvoices.service';

@Component({
  selector: 'app-vendor-invoices',
  imports: [CommonModule,
            DxDataGridModule,
            DxCheckBoxModule,
            DxDateRangeBoxModule],
  templateUrl: 'VendorInvoices.component.html',
  styleUrl: 'VendorInvoices.component.css'
})
export class VendorInvoicesComponent implements OnInit, AfterViewInit {
  loading: boolean = true;

  minDate: Date = new Date(2020, 7, 1);
  startDate: Date = new Date(new Date().setFullYear(new Date().getFullYear() - 1));
  endDate: Date = new Date();
  currentValue: [Date, Date] = [this.startDate, this.endDate];
  gridHeight: number = 0;
  showUnpaid: boolean = false;

  invoices: VendorInvoices[] = [];

  constructor(private vendorInvoicesService: VendorInvoicesService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.FetchInvoices();
  }

  ngAfterViewInit() {
    this.calculateGridHeight();
  }

  @HostListener('window:resize')
  onResize() {
    this.calculateGridHeight();
  }

  calculateGridHeight() {
    this.gridHeight = window.innerHeight - 120;
    this.cdr.detectChanges();
  }

  onCurrentValueChanged(e: DxDateRangeBoxTypes.ValueChangedEvent) {
    this.startDate = e.value[0];
    this.endDate = e.value[1];
    this.FetchInvoices();
    //console.log('Current value changed:', this.currentValue);
  }

  FetchInvoices(): void {
    this.loading = true;

    const invoiceObservable = this.showUnpaid
      ? this.vendorInvoicesService.getUnpaidVendorInvoices(this.startDate, this.endDate, "cdf")
      : this.vendorInvoicesService.getVendorInvoices(this.startDate, this.endDate, "cdf");

    invoiceObservable.subscribe({
      next: (data) => {
        this.invoices = data.map(invoice => ({
          ...invoice,
          paymentDate: invoice.paymentDate === '0001-01-01T00:00:00' ? '' : invoice.paymentDate
        }));
        this.loading = false;
      },
      error: (error) => {
        console.error('Error fetching invoices:', error);
        this.loading = false;
      }
    });
  }
}
