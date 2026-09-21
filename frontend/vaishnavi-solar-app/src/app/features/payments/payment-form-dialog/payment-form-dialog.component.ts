import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { InvoiceService } from '../../../core/services/invoice.service';
import { Invoice } from '../../../core/models/models';
import { PaymentMode } from '../../../core/models/enums';

@Component({
  selector: 'app-payment-form-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatDatepickerModule
  ],
  templateUrl: './payment-form-dialog.component.html',
  styleUrl: './payment-form-dialog.component.scss'
})
export class PaymentFormDialogComponent implements OnInit {
  modes = Object.values(PaymentMode);
  unpaidInvoices: Invoice[] = [];

  form = this.fb.group({
    invoiceId: [null as number | null, Validators.required],
    paymentDate: [new Date(), Validators.required],
    amount: [0, [Validators.required, Validators.min(0.01)]],
    mode: [PaymentMode.Cash, Validators.required],
    referenceNo: [''],
    notes: ['']
  });

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<PaymentFormDialogComponent>,
    private invoiceService: InvoiceService,
    @Inject(MAT_DIALOG_DATA) public data: { invoiceId?: number }
  ) {}

  ngOnInit(): void {
    this.invoiceService.getAll().subscribe((invoices) => {
      this.unpaidInvoices = invoices.filter((i) => i.status !== 'Paid');
      if (this.data?.invoiceId) {
        this.form.patchValue({ invoiceId: this.data.invoiceId });
        this.onInvoiceSelected();
      }
    });
  }

  onInvoiceSelected(): void {
    const inv = this.unpaidInvoices.find((i) => i.id === this.form.value.invoiceId);
    if (inv) {
      this.form.patchValue({ amount: inv.balance });
    }
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.dialogRef.close(this.form.getRawValue());
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
