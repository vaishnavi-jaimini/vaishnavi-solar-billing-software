import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ProformaInvoiceService } from '../../../core/services/proforma-invoice.service';
import { ProformaInvoice } from '../../../core/models/models';

@Component({
  selector: 'app-proforma-invoice-list',
  standalone: true,
  imports: [CommonModule, RouterLink, MatTableModule, MatButtonModule, MatIconModule, MatTooltipModule],
  templateUrl: './proforma-invoice-list.component.html',
  styleUrl: './proforma-invoice-list.component.scss'
})
export class ProformaInvoiceListComponent implements OnInit {
  proformaInvoices: ProformaInvoice[] = [];
  columns = ['proformaNumber', 'date', 'customerName', 'grandTotal', 'actions'];

  constructor(
    private proformaService: ProformaInvoiceService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.proformaService.getAll().subscribe((data) => (this.proformaInvoices = data));
  }

  convert(p: ProformaInvoice): void {
    if (!confirm(`Convert ${p.proformaNumber} to a tax invoice?`)) return;
    this.proformaService.convertToInvoice(p.id).subscribe({
      next: (result) => {
        this.snackBar.open(`Invoice ${result.invoiceNumber} created`, 'Close', { duration: 3000 });
        this.load();
      },
      error: (err) => this.snackBar.open(err.error || 'Could not convert to invoice', 'Close', { duration: 3000 })
    });
  }

  delete(p: ProformaInvoice): void {
    if (!confirm(`Delete proforma invoice ${p.proformaNumber}?`)) return;
    this.proformaService.delete(p.id).subscribe({
      next: () => this.load(),
      error: (err) => this.snackBar.open(err.error || 'Could not delete proforma invoice', 'Close', { duration: 3000 })
    });
  }
}
