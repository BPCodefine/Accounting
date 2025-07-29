import { Component, OnInit, HostListener} from '@angular/core';
import { CommonModule } from '@angular/common';
import { AfterViewInit, ChangeDetectorRef, ElementRef, ViewChild } from '@angular/core';

import { DxDateRangeBoxModule, DxDataGridModule } from 'devextreme-angular';
import { DxDateRangeBoxTypes } from "devextreme-angular/ui/date-range-box"

import { InvExpensesModel } from './InvExpenses.model';
import { InvExpensesService } from './InvExpenses.service';

@Component({
  selector: 'app-inv-expenses-dev-express',
  imports: [CommonModule,
            DxDataGridModule,
            DxDateRangeBoxModule],
  templateUrl: './InvExpenses.component.html',
  styleUrl: './InvExpenses.component.css'
})
export class InvExpensesComponent implements OnInit, AfterViewInit {

  invoices: InvExpensesModel[] = [];
  loading: boolean = true;

  minDate: Date = new Date(2020, 7, 1);
  startDate: Date = new Date(new Date().setFullYear(new Date().getFullYear() - 1));
  endDate: Date = new Date();
  currentValue: [Date, Date] = [this.startDate, this.endDate];
  gridHeight: number = 0;

  @ViewChild('gridWrapper') gridWrapperRef!: ElementRef;

  constructor(private invExpService: InvExpensesService, private cdr: ChangeDetectorRef) {}

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

  onCurrentValueChanged(e: DxDateRangeBoxTypes.ValueChangedEvent) {
    this.startDate = e.value[0];
    this.endDate = e.value[1];
    this.FetchInvoices();
    //console.log('Current value changed:', this.currentValue);
  }

  FetchInvoices() {
    this.loading = true;
    this.invExpService.getInvoices(this.startDate, this.endDate).subscribe({
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

  onCellPrepared(e: any) {
    if (e.rowType === "data") {
      if (e.column.dataField === "puaCost" && e.data.puaCost === 0) {
          e.cellElement.style.cssText = "color: white; background-color: #f5887c";
      }
      if (e.column.dataField === "seafreightCost" && e.data.seafreightCost === 0) {
          e.cellElement.style.cssText = "color: white; background-color: #f5887c";
      }
      if (e.column.dataField === "dutyCost" && e.data.dutyCost === 0) {
          e.cellElement.style.cssText = "color: white; background-color: #f5887c";
      }
    }
  }

  calculateGridHeight() {
    this.gridHeight = window.innerHeight - 120;
    this.cdr.detectChanges();
  }
}
