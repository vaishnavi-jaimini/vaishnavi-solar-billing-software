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
import { ProformaInvoiceService } from '../../../core/services/proforma-invoice.service';
import { CompanyService } from '../../../core/services/company.service';
import { Customer, Item, ProformaInvoice } from '../../../core/models/models';
import { gstRateRows, GstRateRow } from '../../../core/utils/gst-breakup';

@Component({
  selector: 'app-proforma-invoice-form',
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
  templateUrl: './proforma-invoice-form.component.html',
  styleUrl: './proforma-invoice-form.component.scss'
})
export class ProformaInvoiceFormComponent implements OnInit {
  viewMode = false;
  viewProforma: ProformaInvoice | null = null;
  editId: number | null = null;
  editNumber = '';
  viewColumns: string[] = ['item', 'hsn', 'qty', 'rate', 'cgst', 'sgst', 'igst', 'amount'];
  customers: Customer[] = [];
  items: Item[] = [];
  companyState = '';
  saving = false;

  form: FormGroup = this.fb.group({
    proformaNumber: [''],
    customerId: [null, Validators.required],
    proformaDate: [new Date()],
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
    private proformaService: ProformaInvoiceService,
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
      forkJoin([this.customerService.getAll(), this.proformaService.getById(this.editId)]).subscribe(([c, p]) => {
        this.customers = c;
        this.loadForEdit(p);
      });
      return;
    }

    this.customerService.getAll().subscribe((c) => (this.customers = c));
    if (idParam) {
      this.viewMode = true;
      this.proformaService.getById(Number(idParam)).subscribe((p) => {
        this.viewProforma = p;
      });
    } else {
      this.addLine();
    }
  }

  private loadForEdit(p: ProformaInvoice): void {
    if (p.convertedInvoiceId) {
      this.snackBar.open('This proforma invoice is already converted to an invoice and cannot be edited.', 'Close', {
        duration: 3000
      });
      this.router.navigate(['/proforma-invoices', p.id]);
      return;
    }
    this.editNumber = p.proformaNumber;
    const customer = this.customers.find((c) => c.id === p.customerId);
    const shipSameAsBilling =
      !!customer &&
      p.shipToName === customer.name &&
      p.shipToAddress === customer.address &&
      p.shipToCity === customer.city &&
      p.shipToState === customer.state &&
      p.shipToPincode === customer.pincode;
    this.form.get('proformaNumber')?.setValidators(Validators.required);
    this.form.patchValue({
      proformaNumber: p.proformaNumber,
      customerId: p.customerId,
      proformaDate: new Date(p.proformaDate),
      notes: p.notes ?? '',
      shipSameAsBilling,
      shipToName: p.shipToName,
      shipToAddress: p.shipToAddress,
      shipToCity: p.shipToCity,
      shipToState: p.shipToState,
      shipToPincode: p.shipToPincode
    });
    for (const l of p.items) {
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

  gstRows(doc: ProformaInvoice): GstRateRow[] {
    return gstRateRows(doc.items, doc.isInterState);
  }

  // gstRateRows only reads the CGST/SGST or IGST amount that applies, so each line passes all three.
  get formGstRows(): GstRateRow[] {
    const lines = this.lines.controls.map((c, i) => {
      const gst = this.lineGst(i);
      return { gstPercent: c.value.gstPercent, cgstAmount: gst / 2, sgstAmount: gst / 2, igstAmount: gst };
    });
    return gstRateRows(lines, this.isInterState);
  }

  get grandTotal(): number {
    return this.subtotal + this.totalGst;
  }

  convertToInvoice(): void {
    if (!this.viewProforma) return;
    if (!confirm(`Convert ${this.viewProforma.proformaNumber} to a tax invoice?`)) return;
    this.proformaService.convertToInvoice(this.viewProforma.id).subscribe({
      next: (result) => {
        this.snackBar.open(`Invoice ${result.invoiceNumber} created`, 'Close', { duration: 3000 });
        this.router.navigate(['/invoices', result.invoiceId]);
      },
      error: (err) => {
        this.snackBar.open(err.error || 'Could not convert to invoice', 'Close', { duration: 3000 });
      }
    });
  }

  save(): void {
    if (this.form.invalid || this.lines.length === 0) {
      this.form.markAllAsTouched();
      this.snackBar.open('Please select a customer and add at least one item.', 'Close', { duration: 3000 });
      return;
    }
    this.saving = true;
    const value = this.form.getRawValue();
    const proformaDate = value.proformaDate || new Date();
    const dto = {
        proformaNumber: this.editId ? value.proformaNumber : undefined,
        customerId: value.customerId,
        proformaDate,
        notes: value.notes,
        shipSameAsBilling: value.shipSameAsBilling,
        shipToName: value.shipToName,
        shipToAddress: value.shipToAddress,
        shipToCity: value.shipToCity,
        shipToState: value.shipToState,
        shipToPincode: value.shipToPincode,
        items: value.lines
    };
    const request = this.editId ? this.proformaService.update(this.editId, dto) : this.proformaService.create(dto);
    request.subscribe({
      next: (p) => {
        this.snackBar.open(`Proforma Invoice ${p.proformaNumber} ${this.editId ? 'updated' : 'created'}`, 'Close', {
          duration: 3000
        });
        this.router.navigate(['/proforma-invoices', p.id]);
      },
      error: (err) => {
        this.saving = false;
        this.snackBar.open(err.error || 'Could not save proforma invoice', 'Close', { duration: 3000 });
      }
    });
  }
}
