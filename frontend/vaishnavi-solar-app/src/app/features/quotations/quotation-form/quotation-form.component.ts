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
import { CompanyService } from '../../../core/services/company.service';
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
  editId: number | null = null;
  editNumber = '';
  viewQuotation: Quotation | null = null;
  customers: Customer[] = [];
  items: Item[] = [];
  companyState = '';
  saving = false;
  viewColumns = ['item', 'qty', 'rate', 'gst', 'amount', 'cgst', 'sgst', 'igst', 'total'];

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
    private companyService: CompanyService,
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
    this.companyService.get().subscribe((co) => (this.companyState = co.state));
    this.itemService.getAll(undefined, true).subscribe((i) => (this.items = i));

    if (idParam && this.route.snapshot.url.some((s) => s.path === 'edit')) {
      this.editId = Number(idParam);
      this.quotationService.getById(this.editId).subscribe((q) => this.loadForEdit(q));
    } else if (idParam) {
      this.viewMode = true;
      this.quotationService.getById(Number(idParam)).subscribe((q) => (this.viewQuotation = q));
    } else {
      this.addLine();
    }
  }

  private loadForEdit(q: Quotation): void {
    if (q.status === 'Converted') {
      this.snackBar.open('This quotation is already converted to an invoice and cannot be edited.', 'Close', {
        duration: 3000
      });
      this.router.navigate(['/quotations', q.id]);
      return;
    }
    this.editNumber = q.quotationNumber;
    this.form.patchValue({
      customerId: q.customerId,
      date: new Date(q.date),
      validUntil: q.validUntil ? new Date(q.validUntil) : null,
      notes: q.notes ?? ''
    });
    for (const l of q.items) {
      this.addLine();
      this.lines.at(this.lines.length - 1).patchValue({
        itemId: l.itemId,
        description: l.description,
        quantity: l.quantity,
        rate: l.rate,
        gstPercent: l.gstPercent
      });
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
    return Math.round(this.lineAmount(i) * (this.lines.at(i).value.gstPercent || 0)) / 100;
  }

  // Same rule as the backend: customer in the company's state → CGST + SGST, otherwise IGST.
  get customerState(): string {
    const customer = this.customers.find((c) => c.id === this.form.get('customerId')?.value);
    return customer?.state ?? '';
  }

  get isInterState(): boolean {
    return this.customerState.trim().toLowerCase() !== this.companyState.trim().toLowerCase();
  }

  lineCgst(i: number): number {
    return this.isInterState ? 0 : Math.round(this.lineGst(i) * 50) / 100;
  }

  lineSgst(i: number): number {
    return this.isInterState ? 0 : this.lineGst(i) - this.lineCgst(i);
  }

  lineIgst(i: number): number {
    return this.isInterState ? this.lineGst(i) : 0;
  }

  private sumLines(fn: (i: number) => number): number {
    return this.lines.controls.reduce((sum, _, i) => sum + fn(i), 0);
  }

  get subtotal(): number {
    return this.sumLines((i) => this.lineAmount(i));
  }

  get totalCgst(): number {
    return this.sumLines((i) => this.lineCgst(i));
  }

  get totalSgst(): number {
    return this.sumLines((i) => this.lineSgst(i));
  }

  get totalIgst(): number {
    return this.sumLines((i) => this.lineIgst(i));
  }

  get totalGst(): number {
    return this.sumLines((i) => this.lineGst(i));
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
    const dto = {
      customerId: value.customerId,
      date: value.date,
      validUntil: value.validUntil,
      notes: value.notes,
      items: value.lines
    };
    const request = this.editId ? this.quotationService.update(this.editId, dto) : this.quotationService.create(dto);
    request.subscribe({
      next: (q) => {
        this.snackBar.open(`Quotation ${q.quotationNumber} ${this.editId ? 'updated' : 'created'}`, 'Close', {
          duration: 3000
        });
        this.router.navigate(['/quotations']);
      },
      error: (err) => {
        this.saving = false;
        this.snackBar.open(err.error || 'Could not save quotation', 'Close', { duration: 3000 });
      }
    });
  }
}
