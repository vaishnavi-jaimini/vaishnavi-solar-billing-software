import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormArray, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatTableModule } from '@angular/material/table';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CustomerService } from '../../../core/services/customer.service';
import { ItemService } from '../../../core/services/item.service';
import { InvoiceService } from '../../../core/services/invoice.service';
import { CompanyService } from '../../../core/services/company.service';
import { Customer, Invoice, Item } from '../../../core/models/models';

@Component({
  selector: 'app-invoice-form',
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
    MatTableModule,
    MatCheckboxModule
  ],
  templateUrl: './invoice-form.component.html',
  styleUrl: './invoice-form.component.scss'
})
export class InvoiceFormComponent implements OnInit {
  viewMode = false;
  viewInvoice: Invoice | null = null;
  editId: number | null = null;
  editNumber = '';
  viewColumns: string[] = ['item', 'hsn', 'qty', 'rate', 'cgst', 'sgst', 'igst', 'amount'];
  customers: Customer[] = [];
  items: Item[] = [];
  companyState = '';
  saving = false;

  form: FormGroup = this.fb.group({
    customerId: [null, Validators.required],
    invoiceDate: [new Date()],
    placeOfSupply: [''],
    terms: ['Due on Receipt'],
    dueDate: ['As per Agreement'],
    workOrderNo: [''],
    purchaseOrderNo: [''],
    purchaseDate: [null],
    notes: [''],
    shipSameAsBilling: [true],
    shipToName: [''],
    shipToAddress: [''],
    shipToCity: [''],
    shipToState: [''],
    shipToPincode: [''],
    lines: this.fb.array([])
  });

  get lines(): FormArray {
    return this.form.get('lines') as FormArray;
  }

  constructor(
    private fb: FormBuilder,
    private customerService: CustomerService,
    private itemService: ItemService,
    private invoiceService: InvoiceService,
    private companyService: CompanyService,
    private route: ActivatedRoute,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    this.itemService.getAll(undefined, true).subscribe((i) => (this.items = i));
    this.companyService.get().subscribe((c) => (this.companyState = c.state));

    if (idParam && this.route.snapshot.url.some((s) => s.path === 'edit')) {
      this.editId = Number(idParam);
      // Customers are needed to tell whether the saved ship-to address is just the billing address.
      forkJoin([this.customerService.getAll(), this.invoiceService.getById(this.editId)]).subscribe(([c, inv]) => {
        this.customers = c;
        this.loadForEdit(inv);
      });
      return;
    }

    this.customerService.getAll().subscribe((c) => (this.customers = c));
    if (idParam) {
      this.viewMode = true;
      this.invoiceService.getById(Number(idParam)).subscribe((inv) => {
        this.viewInvoice = inv;
      });
    } else {
      this.addLine();
    }
  }

  private loadForEdit(inv: Invoice): void {
    this.editNumber = inv.invoiceNumber;
    const customer = this.customers.find((c) => c.id === inv.customerId);
    const shipSameAsBilling =
      !!customer &&
      inv.shipToName === customer.name &&
      inv.shipToAddress === customer.address &&
      inv.shipToCity === customer.city &&
      inv.shipToState === customer.state &&
      inv.shipToPincode === customer.pincode;
    this.form.patchValue({
      customerId: inv.customerId,
      invoiceDate: new Date(inv.invoiceDate),
      placeOfSupply: inv.placeOfSupply,
      terms: inv.terms,
      dueDate: inv.dueDate,
      workOrderNo: inv.workOrderNo,
      purchaseOrderNo: inv.purchaseOrderNo,
      purchaseDate: inv.purchaseDate ? new Date(inv.purchaseDate) : null,
      notes: inv.notes ?? '',
      shipSameAsBilling,
      shipToName: inv.shipToName,
      shipToAddress: inv.shipToAddress,
      shipToCity: inv.shipToCity,
      shipToState: inv.shipToState,
      shipToPincode: inv.shipToPincode
    });
    for (const l of inv.items) {
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

  get isInterState(): boolean {
    const customerId = this.form.get('customerId')?.value;
    const customer = this.customers.find((c) => c.id === customerId);
    if (!customer || !this.companyState) return false;
    return customer.state?.trim().toLowerCase() !== this.companyState.trim().toLowerCase();
  }

  onCustomerSelected(): void {
    const customer = this.customers.find((c) => c.id === this.form.get('customerId')?.value);
    if (customer) {
      this.form.patchValue({ placeOfSupply: customer.state });
    }
    if (this.form.get('shipSameAsBilling')?.value) {
      this.copyBillingToShipTo();
    }
  }

  onShipSameAsBillingChange(checked: boolean): void {
    if (checked) {
      this.copyBillingToShipTo();
    }
  }

  private copyBillingToShipTo(): void {
    const customerId = this.form.get('customerId')?.value;
    const customer = this.customers.find((c) => c.id === customerId);
    if (!customer) return;
    this.form.patchValue({
      shipToName: customer.name,
      shipToAddress: customer.address,
      shipToCity: customer.city,
      shipToState: customer.state,
      shipToPincode: customer.pincode
    });
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

  get cgst(): number {
    return this.isInterState ? 0 : this.totalGst / 2;
  }

  get sgst(): number {
    return this.isInterState ? 0 : this.totalGst / 2;
  }

  get igst(): number {
    return this.isInterState ? this.totalGst : 0;
  }

  get grandTotal(): number {
    return this.subtotal + this.totalGst;
  }

  // Send the picked calendar day as yyyy-MM-dd so the timezone can't shift it to the previous day.
  private toDateOnly(d: Date | null): string | null {
    if (!d) return null;
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
  }

  save(): void {
    if (this.form.invalid || this.lines.length === 0) {
      this.form.markAllAsTouched();
      this.snackBar.open('Please select a customer and add at least one item.', 'Close', { duration: 3000 });
      return;
    }
    this.saving = true;
    const value = this.form.getRawValue();
    const invoiceDate = value.invoiceDate || new Date();
    const dto = {
        customerId: value.customerId,
        invoiceDate,
        notes: value.notes,
        shipSameAsBilling: value.shipSameAsBilling,
        shipToName: value.shipToName,
        shipToAddress: value.shipToAddress,
        shipToCity: value.shipToCity,
        shipToState: value.shipToState,
        shipToPincode: value.shipToPincode,
        placeOfSupply: value.placeOfSupply,
        terms: value.terms,
        dueDate: value.dueDate,
        workOrderNo: value.workOrderNo,
        purchaseOrderNo: value.purchaseOrderNo,
        purchaseDate: this.toDateOnly(value.purchaseDate),
        items: value.lines
    };
    const request = this.editId ? this.invoiceService.update(this.editId, dto) : this.invoiceService.create(dto);
    request.subscribe({
      next: (inv) => {
        this.snackBar.open(`Invoice ${inv.invoiceNumber} ${this.editId ? 'updated' : 'created'}`, 'Close', {
          duration: 3000
        });
        this.router.navigate(['/invoices', inv.id]);
      },
      error: (err) => {
        this.saving = false;
        this.snackBar.open(err.error || 'Could not save invoice', 'Close', { duration: 3000 });
      }
    });
  }
}
