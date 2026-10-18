import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CompanyService } from '../../../core/services/company.service';
import { INDIAN_STATES } from '../../../core/models/enums';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss'
})
export class SettingsComponent implements OnInit {
  states = INDIAN_STATES;
  saving = false;

  form = this.fb.group({
    id: [0],
    name: ['', Validators.required],
    address: [''],
    city: [''],
    state: ['Maharashtra', Validators.required],
    pincode: [''],
    gstin: [''],
    phone: [''],
    email: [''],
    bankName: [''],
    bankAccountNo: [''],
    bankIfsc: [''],
    bankAccountName: [''],
    bankBranch: [''],
    invoicePrefix: ['INV-', Validators.required]
  });

  constructor(
    private fb: FormBuilder,
    private companyService: CompanyService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.companyService.get().subscribe((c) => this.form.patchValue(c));
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving = true;
    this.companyService.update(this.form.getRawValue() as any).subscribe({
      next: () => {
        this.saving = false;
        this.snackBar.open('Settings saved', 'Close', { duration: 2500 });
      },
      error: () => {
        this.saving = false;
        this.snackBar.open('Could not save settings', 'Close', { duration: 2500 });
      }
    });
  }
}
