import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { Item } from '../../../core/models/models';
import { StockTransactionType } from '../../../core/models/enums';

@Component({
  selector: 'app-stock-adjust-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule
  ],
  templateUrl: './stock-adjust-dialog.component.html',
  styleUrl: './stock-adjust-dialog.component.scss'
})
export class StockAdjustDialogComponent {
  types = [StockTransactionType.Purchase, StockTransactionType.Adjustment, StockTransactionType.Sale];

  form = this.fb.group({
    type: [StockTransactionType.Purchase, Validators.required],
    quantity: [0, [Validators.required, Validators.min(0.001)]],
    notes: ['']
  });

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<StockAdjustDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { item: Item }
  ) {}

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.dialogRef.close({ itemId: this.data.item.id, ...this.form.getRawValue() });
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
