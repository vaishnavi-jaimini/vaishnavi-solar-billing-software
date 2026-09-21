import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { InvoiceService } from '../../../core/services/invoice.service';
import { CompanyService } from '../../../core/services/company.service';
import { Company, Invoice } from '../../../core/models/models';
import { numberToIndianWords } from '../../../core/utils/number-to-words';

@Component({
  selector: 'app-invoice-print',
  standalone: true,
  imports: [CommonModule, RouterLink, MatButtonModule, MatIconModule],
  templateUrl: './invoice-print.component.html',
  styleUrl: './invoice-print.component.scss'
})
export class InvoicePrintComponent implements OnInit {
  invoice: Invoice | null = null;
  company: Company | null = null;

  constructor(
    private route: ActivatedRoute,
    private invoiceService: InvoiceService,
    private companyService: CompanyService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.invoiceService.getById(id).subscribe((inv) => (this.invoice = inv));
    this.companyService.get().subscribe((c) => (this.company = c));
  }

  amountInWords(amount: number): string {
    return numberToIndianWords(amount);
  }

  print(): void {
    window.print();
  }
}
