import { Component, OnInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AfterViewInit, ChangeDetectorRef, ElementRef, ViewChild } from '@angular/core';
import { DxDateBoxModule, DxTextBoxModule, DxDataGridModule } from 'devextreme-angular';
import { DxTextBoxTypes } from 'devextreme-angular/ui/text-box'
import { DxDataGridTypes } from 'devextreme-angular/ui/data-grid';
import { Workbook } from 'exceljs';
import { saveAs } from 'file-saver';

import { exportDataGrid } from 'devextreme-angular/common/export/excel';
import { BankAccountLedgerModel } from './BankAccountLedger.model';
import { BankAccountLedgerService } from './BankAccountLedger.service';

@Component({
  selector: 'app-bank-account-ledger',
  imports: [CommonModule,
            DxDateBoxModule,
            DxTextBoxModule,
             DxDataGridModule],
  templateUrl: './BankAccountLedger.component.html',
  styleUrl: './BankAccountLedger.component.css'
})
export class BankAccountLedgerComponent implements OnInit, AfterViewInit  {
  loading: boolean = true;
  endDate: Date = new Date();
  accountsText: string = '';
  defAccounts: string[] = [];
  ledgerLines : BankAccountLedgerModel[] = [];
  gridHeight: number = 0;

  @ViewChild('gridWrapper') gridWrapperRef!: ElementRef;

  constructor( private ledgerService: BankAccountLedgerService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.FetchLedgerLines();
  }

  ngAfterViewInit() {
    this.calculateGridHeight();
  }

  @HostListener('window:resize')
  onResize() {
    this.calculateGridHeight();
  }

  accListValueChanged(data: DxTextBoxTypes.ValueChangedEvent) {
    this.defAccounts = this.accountsText.split(',').map(acc => acc.trim());
  }

  FetchLedgerLines(): void {
    this.loading = true;
    this.ledgerService.getLedgerEntries(this.endDate, this.defAccounts).subscribe(
      /*response => {console.log('✅ Response:', response);}*/
      {
      next: (data) => {
        this.ledgerLines = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error fetching invoices:', error);
        this.loading = false;
      },
    });
    console.log('Fetching invoices...');
  }

  onExporting(e: DxDataGridTypes.ExportingEvent) {
    const workbook = new Workbook();
    const worksheet = workbook.addWorksheet('Data');

    exportDataGrid({
      component: e.component,
      worksheet,
      autoFilterEnabled: true,
    }).then(() => {
      workbook.xlsx.writeBuffer().then((buffer) => {
        saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'DataGrid.xlsx');
      });
    });
  }

  calculateGridHeight () {
    this.gridHeight = window.innerHeight - 150;
    this.cdr.detectChanges();
  }
}
