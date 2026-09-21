import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBar } from '@angular/material/snack-bar';
import { QuotationService } from '../../../core/services/quotation.service';
import { InvoiceService } from '../../../core/services/invoice.service';
import { Quotation } from '../../../core/models/models';
import { QuotationStatus } from '../../../core/models/enums';

@Component({
  selector: 'app-quotation-list',
  standalone: true,
  imports: [CommonModule, RouterLink, MatTableModule, MatButtonModule, MatIconModule, MatMenuModule],
  templateUrl: './quotation-list.component.html',
  styleUrl: './quotation-list.component.scss'
})
export class QuotationListComponent implements OnInit {
  quotations: Quotation[] = [];
  columns = ['quotationNumber', 'date', 'customerName', 'grandTotal', 'status', 'actions'];
  statuses = Object.values(QuotationStatus);

  constructor(
    private quotationService: QuotationService,
    private invoiceService: InvoiceService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.quotationService.getAll().subscribe((data) => (this.quotations = data));
  }

  setStatus(q: Quotation, status: QuotationStatus): void {
    this.quotationService.updateStatus(q.id, status).subscribe(() => this.load());
  }

  markSent(q: Quotation): void {
    this.setStatus(q, QuotationStatus.Sent);
  }

  markAccepted(q: Quotation): void {
    this.setStatus(q, QuotationStatus.Accepted);
  }

  markRejected(q: Quotation): void {
    this.setStatus(q, QuotationStatus.Rejected);
  }

  convert(q: Quotation): void {
    if (!confirm(`Convert quotation ${q.quotationNumber} to an invoice?`)) return;
    this.invoiceService.convertQuotation(q.id).subscribe({
      next: (inv) => {
        this.snackBar.open(`Invoice ${inv.invoiceNumber} created`, 'Close', { duration: 3000 });
        this.load();
      },
      error: (err) => this.snackBar.open(err.error || 'Could not convert quotation', 'Close', { duration: 3000 })
    });
  }

  delete(q: Quotation): void {
    if (!confirm(`Delete quotation ${q.quotationNumber}?`)) return;
    this.quotationService.delete(q.id).subscribe({
      next: () => this.load(),
      error: (err) => this.snackBar.open(err.error || 'Could not delete quotation', 'Close', { duration: 3000 })
    });
  }

  statusClass(status: string): string {
    return `status-chip status-${status.toLowerCase()}`;
  }
}
