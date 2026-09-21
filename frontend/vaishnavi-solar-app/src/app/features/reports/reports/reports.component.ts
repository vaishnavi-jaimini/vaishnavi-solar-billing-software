import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { ReportService } from '../../../core/services/report.service';
import { GstSummary, OutstandingDueRow, SalesReport, StockReportRow } from '../../../core/models/models';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTabsModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatDatepickerModule,
    MatCheckboxModule
  ],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.scss'
})
export class ReportsComponent implements OnInit {
  from: Date | null = null;
  to: Date | null = null;

  salesReport: SalesReport | null = null;
  gstSummary: GstSummary | null = null;
  outstandingDues: OutstandingDueRow[] = [];
  stockRows: StockReportRow[] = [];
  lowOnly = false;

  salesColumns = ['invoiceNumber', 'invoiceDate', 'customerName', 'subtotal', 'totalGst', 'grandTotal', 'status'];
  gstColumns = ['hsnSacCode', 'itemName', 'taxableValue', 'cgstAmount', 'sgstAmount', 'igstAmount', 'totalTax'];
  duesColumns = ['invoiceNumber', 'invoiceDate', 'customerName', 'customerPhone', 'grandTotal', 'amountPaid', 'balance'];
  stockColumns = ['itemName', 'category', 'currentStock', 'lowStockThreshold'];

  constructor(private reportService: ReportService) {}

  ngOnInit(): void {
    this.loadSales();
    this.loadGst();
    this.loadDues();
    this.loadStock();
  }

  private dateParam(d: Date | null): string | undefined {
    return d ? d.toISOString() : undefined;
  }

  loadSales(): void {
    this.reportService.sales(this.dateParam(this.from), this.dateParam(this.to)).subscribe((r) => (this.salesReport = r));
  }

  loadGst(): void {
    this.reportService.gstSummary(this.dateParam(this.from), this.dateParam(this.to)).subscribe((r) => (this.gstSummary = r));
  }

  loadDues(): void {
    this.reportService.outstandingDues().subscribe((r) => (this.outstandingDues = r));
  }

  loadStock(): void {
    this.reportService.stock(this.lowOnly).subscribe((r) => (this.stockRows = r));
  }

  applyDateFilter(): void {
    this.loadSales();
    this.loadGst();
  }
}
