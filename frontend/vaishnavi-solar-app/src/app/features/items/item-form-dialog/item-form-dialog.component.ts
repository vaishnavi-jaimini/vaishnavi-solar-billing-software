import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { Item } from '../../../core/models/models';
import { ItemCategory, ItemCategoryLabels, ItemUnit } from '../../../core/models/enums';

@Component({
  selector: 'app-item-form-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatCheckboxModule
  ],
  templateUrl: './item-form-dialog.component.html',
  styleUrl: './item-form-dialog.component.scss'
})
export class ItemFormDialogComponent {
  categories = Object.values(ItemCategory);
  categoryLabels = ItemCategoryLabels;
  units = Object.values(ItemUnit);

  form = this.fb.group({
    name: ['', Validators.required],
    category: [ItemCategory.Panel, Validators.required],
    hsnSacCode: ['', Validators.required],
    unit: [ItemUnit.Nos, Validators.required],
    rate: [0, [Validators.required, Validators.min(0)]],
    gstPercent: [18, [Validators.required, Validators.min(0), Validators.max(100)]],
    lowStockThreshold: [5, [Validators.min(0)]],
    isService: [false],
    isActive: [true]
  });

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<ItemFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { item: Item | null }
  ) {
    if (data.item) {
      this.form.patchValue(data.item);
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
