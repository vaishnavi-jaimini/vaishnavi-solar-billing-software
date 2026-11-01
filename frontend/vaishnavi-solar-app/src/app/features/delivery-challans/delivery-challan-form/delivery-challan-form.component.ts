import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormArray, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatTableModule } from '@angular/material/table';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CustomerService } from '../../../core/services/customer.service';
import { ItemService } from '../../../core/services/item.service';
import { DeliveryChallanService } from '../../../core/services/delivery-challan.service';
import { InvoiceService } from '../../../core/services/invoice.service';
import { CompanyService } from '../../../core/services/company.service';
import { Customer, DeliveryChallan, Invoice, Item } from '../../../core/models/models';
import { ItemUnit } from '../../../core/models/enums';
import { gstRateRows, GstRateRow } from '../../../core/utils/gst-breakup';

@Component({
  selector: 'app-delivery-challan-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatIconModule,
    MatDatepickerModule,
    MatTableModule,
    MatCheckboxModule
  ],
  templateUrl: './delivery-challan-form.component.html',
  styleUrl: './delivery-challan-form.component.scss'
})
export class DeliveryChallanFormComponent implements OnInit {
  viewMode = false;
  viewChallan: DeliveryChallan | null = null;
  editId: number | null = null;
  editNumber = '';
  viewColumns: string[] = ['item', 'hsn', 'qty', 'rate', 'cgst', 'sgst', 'igst', 'amount'];
  customers: Customer[] = [];
  items: Item[] = [];
  units = Object.values(ItemUnit);
  companyState = '';
  saving = false;

  form: FormGroup = this.fb.group({
    challanNumber: [''],
    customerId: [null, Validators.required],
    challanDate: [new Date()],
    referenceNo: [''],
    transporterName: [''],
    vehicleNumber: [''],
    lrNumber: [''],
    ewayBillNumber: [''],
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
    private challanService: DeliveryChallanService,
    private invoiceService: InvoiceService,
    private companyService: CompanyService,
    private route: ActivatedRoute,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    const invoiceIdParam = this.route.snapshot.queryParamMap.get('invoiceId');
    this.itemService.getAll(undefined, true).subscribe((i) => (this.items = i));
    this.companyService.get().subscribe((c) => (this.companyState = c.state));

    if (idParam && this.route.snapshot.url.some((s) => s.path === 'edit')) {
      this.editId = Number(idParam);
      // Customers are needed to tell whether the saved ship-to address is just the billing address.
      forkJoin([this.customerService.getAll(), this.challanService.getById(this.editId)]).subscribe(([customers, c]) => {
        this.customers = customers;
        this.editNumber = c.challanNumber;
        this.form.get('challanNumber')?.setValidators(Validators.required);
        this.form.patchValue({
          challanNumber: c.challanNumber,
          challanDate: new Date(c.challanDate),
          referenceNo: c.referenceNo,
          transporterName: c.transporterName,
          vehicleNumber: c.vehicleNumber,
          lrNumber: c.lrNumber,
          ewayBillNumber: c.ewayBillNumber,
          notes: c.notes ?? ''
        });
        this.loadDetails(c);
      });
      return;
    }

    if (invoiceIdParam) {
      // Starting from an invoice: the goods, customer and ship-to address are copied over and can still be changed.
      forkJoin([this.customerService.getAll(), this.invoiceService.getById(Number(invoiceIdParam))]).subscribe(
        ([customers, invoice]) => {
          this.customers = customers;
          this.form.patchValue({ referenceNo: invoice.invoiceNumber, notes: invoice.notes ?? '' });
          this.loadDetails(invoice);
        }
      );
      return;
    }

    this.customerService.getAll().subscribe((c) => (this.customers = c));
    if (idParam) {
      this.viewMode = true;
      this.challanService.getById(Number(idParam)).subscribe((c) => {
        this.viewChallan = c;
      });
    } else {
      this.addLine();
    }
  }

  // Fills what a challan shares with the document it is loaded from: customer, ship-to address and lines.
  private loadDetails(doc: DeliveryChallan | Invoice): void {
    const customer = this.customers.find((c) => c.id === doc.customerId);
    const shipSameAsBilling =
      !!customer &&
      doc.shipToName === customer.name &&
      doc.shipToAddress === customer.address &&
      doc.shipToCity === customer.city &&
      doc.shipToState === customer.state &&
      doc.shipToPincode === customer.pincode;
    this.form.patchValue({
      customerId: doc.customerId,
      shipSameAsBilling,
      shipToName: doc.shipToName,
      shipToAddress: doc.shipToAddress,
      shipToCity: doc.shipToCity,
      shipToState: doc.shipToState,
      shipToPincode: doc.shipToPincode
    });
    for (const l of doc.items) {
      this.addLine();
      this.lines.at(this.lines.length - 1).patchValue({
        itemId: l.itemId,
        description: l.description,
        hsnSacCode: l.hsnSacCode,
        unit: l.unit,
        quantity: l.quantity,
        rate: l.rate,
        gstPercent: l.gstPercent
      });
    }
  }

  addLine(): void {
    this.lines.push(
      this.fb.group({
        itemId: [null as number | null],
        description: ['', Validators.required],
        hsnSacCode: [''],
        unit: [ItemUnit.Nos],
        quantity: [1, [Validators.required, Validators.min(0.001)]],
        rate: [0, [Validators.required, Validators.min(0)]],
        gstPercent: [18, [Validators.required, Validators.min(0)]]
      })
    );
  }

  removeLine(i: number): void {
    this.lines.removeAt(i);
  }

  // Stored items are only suggestions: the product, HSN, unit, rate and GST can all be typed in by hand.
  matchingItems(i: number): Item[] {
    const typed = (this.lines.at(i).get('description')?.value || '').trim().toLowerCase();
    return this.items.filter((it) => it.name.toLowerCase().includes(typed)).slice(0, 10);
  }

  onItemPicked(i: number, item: Item): void {
    this.lines.at(i).patchValue({
      itemId: item.id,
      description: item.name,
      hsnSacCode: item.hsnSacCode,
      unit: item.unit,
      rate: item.rate,
      gstPercent: item.gstPercent
    });
  }

  // Once the name no longer matches the picked item, the line is a typed-in product.
  onProductTyped(i: number): void {
    const line = this.lines.at(i);
    const item = this.items.find((x) => x.id === line.get('itemId')?.value);
    if (item && item.name !== line.get('description')?.value) {
      line.patchValue({ itemId: null });
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

  gstRows(doc: DeliveryChallan): GstRateRow[] {
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

  save(): void {
    if (this.form.invalid || this.lines.length === 0) {
      this.form.markAllAsTouched();
      this.snackBar.open('Please select a customer and add at least one item.', 'Close', { duration: 3000 });
      return;
    }
    this.saving = true;
    const value = this.form.getRawValue();
    const challanDate = value.challanDate || new Date();
    const dto = {
      challanNumber: this.editId ? value.challanNumber : undefined,
      customerId: value.customerId,
      challanDate,
      notes: value.notes,
      shipSameAsBilling: value.shipSameAsBilling,
      shipToName: value.shipToName,
      shipToAddress: value.shipToAddress,
      shipToCity: value.shipToCity,
      shipToState: value.shipToState,
      shipToPincode: value.shipToPincode,
      referenceNo: value.referenceNo,
      transporterName: value.transporterName,
      vehicleNumber: value.vehicleNumber,
      lrNumber: value.lrNumber,
      ewayBillNumber: value.ewayBillNumber,
      items: value.lines
    };
    const request = this.editId ? this.challanService.update(this.editId, dto) : this.challanService.create(dto);
    request.subscribe({
      next: (c) => {
        this.snackBar.open(`Delivery Challan ${c.challanNumber} ${this.editId ? 'updated' : 'created'}`, 'Close', {
          duration: 3000
        });
        this.router.navigate(['/delivery-challans', c.id]);
      },
      error: (err) => {
        this.saving = false;
        this.snackBar.open(err.error || 'Could not save delivery challan', 'Close', { duration: 3000 });
      }
    });
  }
}
