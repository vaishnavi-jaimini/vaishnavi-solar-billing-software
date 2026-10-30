import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { InvoiceService } from '../../../core/services/invoice.service';
import { Invoice } from '../../../core/models/models';

@Component({
  selector: 'app-invoice-list',
  standalone: true,
  imports: [CommonModule, RouterLink, MatTableModule, MatButtonModule, MatIconModule, MatSnackBarModule],
  templateUrl: './invoice-list.component.html',
  styleUrl: './invoice-list.component.scss'
})
export class InvoiceListComponent implements OnInit {
  invoices: Invoice[] = [];
  columns = ['invoiceNumber', 'date', 'customerName', 'grandTotal', 'balance', 'actions'];

  constructor(
    private invoiceService: InvoiceService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.invoiceService.getAll().subscribe((data) => (this.invoices = data));
  }

  delete(i: Invoice): void {
    if (!confirm(`Delete invoice ${i.invoiceNumber}? Its items will be returned to stock.`)) return;
    this.invoiceService.delete(i.id).subscribe({
      next: () => {
        this.snackBar.open(`Invoice ${i.invoiceNumber} deleted`, 'Close', { duration: 2500 });
        this.load();
      },
      error: (err) => this.snackBar.open(err.error || 'Could not delete invoice', 'Close', { duration: 3000 })
    });
  }
}
