import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PaymentService } from '../../../core/services/payment.service';
import { Payment } from '../../../core/models/models';
import { PaymentFormDialogComponent } from '../payment-form-dialog/payment-form-dialog.component';

@Component({
  selector: 'app-payment-list',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule, MatIconModule, MatDialogModule],
  templateUrl: './payment-list.component.html',
  styleUrl: './payment-list.component.scss'
})
export class PaymentListComponent implements OnInit {
  payments: Payment[] = [];
  columns = ['paymentDate', 'invoiceNumber', 'customerName', 'amount', 'mode', 'referenceNo', 'actions'];

  constructor(
    private paymentService: PaymentService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.paymentService.getAll().subscribe((data) => (this.payments = data));
  }

  openForm(): void {
    const ref = this.dialog.open(PaymentFormDialogComponent, { width: '640px', data: {} });
    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.paymentService.create(result).subscribe(() => {
        this.snackBar.open('Payment recorded', 'Close', { duration: 2500 });
        this.load();
      });
    });
  }

  delete(payment: Payment): void {
    if (!confirm('Delete this payment record?')) return;
    this.paymentService.delete(payment.id).subscribe(() => {
      this.snackBar.open('Payment deleted', 'Close', { duration: 2500 });
      this.load();
    });
  }
}
