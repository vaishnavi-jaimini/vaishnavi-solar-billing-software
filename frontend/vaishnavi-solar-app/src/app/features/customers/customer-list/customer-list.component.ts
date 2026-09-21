import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CustomerService } from '../../../core/services/customer.service';
import { Customer } from '../../../core/models/models';
import { CustomerFormDialogComponent } from '../customer-form-dialog/customer-form-dialog.component';

@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatDialogModule
  ],
  templateUrl: './customer-list.component.html',
  styleUrl: './customer-list.component.scss'
})
export class CustomerListComponent implements OnInit {
  customers: Customer[] = [];
  search = '';
  columns = ['name', 'phone', 'city', 'state', 'gstin', 'actions'];

  constructor(
    private customerService: CustomerService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.customerService.getAll(this.search || undefined).subscribe((data) => (this.customers = data));
  }

  openForm(customer: Customer | null): void {
    const ref = this.dialog.open(CustomerFormDialogComponent, {
      width: '640px',
      data: { customer }
    });
    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      const request = customer
        ? this.customerService.update(customer.id, result)
        : this.customerService.create(result);
      request.subscribe(() => {
        this.snackBar.open(customer ? 'Customer updated' : 'Customer added', 'Close', { duration: 2500 });
        this.load();
      });
    });
  }

  delete(customer: Customer): void {
    if (!confirm(`Delete customer "${customer.name}"?`)) return;
    this.customerService.delete(customer.id).subscribe({
      next: () => {
        this.snackBar.open('Customer deleted', 'Close', { duration: 2500 });
        this.load();
      },
      error: (err) => {
        this.snackBar.open(err.error || 'Could not delete customer', 'Close', { duration: 3000 });
      }
    });
  }
}
