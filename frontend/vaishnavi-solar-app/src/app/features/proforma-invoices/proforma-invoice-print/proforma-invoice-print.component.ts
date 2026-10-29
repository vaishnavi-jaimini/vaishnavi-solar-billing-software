import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ProformaInvoiceService } from '../../../core/services/proforma-invoice.service';
import { CompanyService } from '../../../core/services/company.service';
import { Company, ProformaInvoice } from '../../../core/models/models';
import { numberToIndianWords } from '../../../core/utils/number-to-words';
import { gstRateRows, GstRateRow } from '../../../core/utils/gst-breakup';

@Component({
  selector: 'app-proforma-invoice-print',
  standalone: true,
  imports: [CommonModule, RouterLink, MatButtonModule, MatIconModule],
  templateUrl: './proforma-invoice-print.component.html',
  styleUrl: './proforma-invoice-print.component.scss'
})
export class ProformaInvoicePrintComponent implements OnInit {
  proforma: ProformaInvoice | null = null;
  company: Company | null = null;

  constructor(
    private route: ActivatedRoute,
    private proformaService: ProformaInvoiceService,
    private companyService: CompanyService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.proformaService.getById(id).subscribe((p) => (this.proforma = p));
    this.companyService.get().subscribe((c) => (this.company = c));
  }

  gstRows(doc: ProformaInvoice): GstRateRow[] {
    return gstRateRows(doc.items, doc.isInterState);
  }

  amountInWords(amount: number): string {
    return numberToIndianWords(amount);
  }

  print(): void {
    window.print();
  }
}
