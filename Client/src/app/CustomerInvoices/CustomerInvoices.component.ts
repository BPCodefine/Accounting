import { Component, OnInit, HostListener, ChangeDetectorRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DxSelectBoxModule, DxDateRangeBoxModule, DxCheckBoxModule, DxDataGridModule } from 'devextreme-angular';
import { DxDateRangeBoxTypes } from "devextreme-angular/ui/date-range-box"

import { CustInvoices } from './CustInvoices.model';
import { CustInvoiceService } from './CustInvoices.service';
import { UserService } from '../GeneralData/WinUserName.service';

@Component({
  selector: 'app-customer-invoices',
  imports: [CommonModule,
            DxSelectBoxModule,
            DxDataGridModule,
            DxCheckBoxModule,
            DxDateRangeBoxModule],
  templateUrl: './CustomerInvoices.component.html',
  styleUrl: './CustomerInvoices.component.css'
})
export class CustomerInvoicesComponent implements OnInit, AfterViewInit {
  username = '';
  loading: boolean = true;

  minDate: Date = new Date(2020, 7, 1);
  startDate: Date = new Date(new Date().setFullYear(new Date().getFullYear() - 1));
  endDate: Date = new Date();
  currentValue: [Date, Date] = [this.startDate, this.endDate];
  gridHeight: number = 0;
  showOverdue: boolean = false;

  invoices: CustInvoices[] = [];
  enabledComps: string[] = [];
  selectedComp!: string;

  constructor(private userService: UserService, private custInvoicesService: CustInvoiceService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.userService.getUsername().subscribe({
      next: res => {
        this.username = res;

        this.userService.getEnabledCompanies(this.username).subscribe({
          next: comps => {
            this.enabledComps = comps;

            if (this.enabledComps.length > 0) {
              this.selectedComp = this.enabledComps[0];
            }
            this.FetchInvoices();
          },
          error: err => {
            console.error('Failed to get enabled companies', err);
          }
        });
      },
      error: err => {
        console.error('Failed to get username', err);
        this.username = 'Unknown User';
      }
    });
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
  }

  FetchInvoices() {
    this.loading = true;

    const invoiceObservable = this.showOverdue
      ? this.custInvoicesService.getLateCustInvoices(this.selectedComp)
      : this.custInvoicesService.getCustInvoices(this.startDate, this.endDate, this.selectedComp);

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
