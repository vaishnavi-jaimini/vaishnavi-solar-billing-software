import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormArray, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatTableModule } from '@angular/material/table';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CustomerService } from '../../../core/services/customer.service';
import { ItemService } from '../../../core/services/item.service';
import { QuotationService } from '../../../core/services/quotation.service';
import { Customer, Item, Quotation } from '../../../core/models/models';

@Component({
  selector: 'app-quotation-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatDatepickerModule,
    MatTableModule
  ],
  templateUrl: './quotation-form.component.html',
  styleUrl: './quotation-form.component.scss'
})
export class QuotationFormComponent implements OnInit {
  viewMode = false;
  viewQuotation: Quotation | null = null;
  customers: Customer[] = [];
  items: Item[] = [];
  saving = false;

  form: FormGroup = this.fb.group({
    customerId: [null, Validators.required],
    date: [new Date(), Validators.required],
    validUntil: [null],
    notes: [''],
    lines: this.fb.array([])
  });

  get lines(): FormArray {
    return this.form.get('lines') as FormArray;
  }

  constructor(
    private fb: FormBuilder,
    private customerService: CustomerService,
    private itemService: ItemService,
    private quotationService: QuotationService,
    private route: ActivatedRoute,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    this.customerService.getAll().subscribe((c) => (this.customers = c));
    this.itemService.getAll(undefined, true).subscribe((i) => (this.items = i));

    if (idParam) {
      this.viewMode = true;
      this.quotationService.getById(Number(idParam)).subscribe((q) => (this.viewQuotation = q));
    } else {
      this.addLine();
    }
  }

  addLine(): void {
    this.lines.push(
      this.fb.group({
        itemId: [null, Validators.required],
        description: [''],
        quantity: [1, [Validators.required, Validators.min(0.001)]],
        rate: [0, [Validators.required, Validators.min(0)]],
        gstPercent: [18, [Validators.required, Validators.min(0)]]
      })
    );
  }

  removeLine(i: number): void {
    this.lines.removeAt(i);
  }

  onItemSelected(i: number): void {
    const itemId = this.lines.at(i).get('itemId')?.value;
    const item = this.items.find((x) => x.id === itemId);
    if (item) {
      this.lines.at(i).patchValue({
        rate: item.rate,
        gstPercent: item.gstPercent,
        description: item.name
      });
    }
  }

  lineAmount(i: number): number {
    const line = this.lines.at(i).value;
    return (line.quantity || 0) * (line.rate || 0);
  }

  lineGst(i: number): number {
    return (this.lineAmount(i) * (this.lines.at(i).value.gstPercent || 0)) / 100;
  }

  get subtotal(): number {
    return this.lines.controls.reduce((sum, _, i) => sum + this.lineAmount(i), 0);
  }

  get totalGst(): number {
    return this.lines.controls.reduce((sum, _, i) => sum + this.lineGst(i), 0);
  }

  get grandTotal(): number {
    return this.subtotal + this.totalGst;
  }

  save(): void {
    if (this.form.invalid || this.lines.length === 0) {
      this.form.markAllAsTouched();
      this.snackBar.open('Please select a customer and add at least one item.', 'Close', { duration: 3000 });
      return;
    }
    this.saving = true;
    const value = this.form.getRawValue();
    this.quotationService
      .create({
        customerId: value.customerId,
        date: value.date,
        validUntil: value.validUntil,
        notes: value.notes,
        items: value.lines
      })
      .subscribe({
        next: (q) => {
          this.snackBar.open(`Quotation ${q.quotationNumber} created`, 'Close', { duration: 3000 });
          this.router.navigate(['/quotations']);
        },
        error: (err) => {
          this.saving = false;
          this.snackBar.open(err.error || 'Could not create quotation', 'Close', { duration: 3000 });
        }
      });
  }
}
