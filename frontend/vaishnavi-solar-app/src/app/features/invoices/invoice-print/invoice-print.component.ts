import { Component, ElementRef, HostListener, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { InvoiceService } from '../../../core/services/invoice.service';
import { CompanyService } from '../../../core/services/company.service';
import { Company, Invoice } from '../../../core/models/models';
import { numberToIndianWords } from '../../../core/utils/number-to-words';
import { gstRateRows, GstRateRow } from '../../../core/utils/gst-breakup';

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
  @ViewChild('sheet') sheet?: ElementRef<HTMLElement>;

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

  gstRows(doc: Invoice): GstRateRow[] {
    return gstRateRows(doc.items, doc.isInterState);
  }

  amountInWords(amount: number): string {
    return numberToIndianWords(amount);
  }

  print(): void {
    window.print();
  }

  // Shrinks the sheet just enough to fit one A4 page. It is measured at the printed width (A4 at 96dpi),
  // since the on-screen sheet is wider and would come out shorter than it really prints.
  @HostListener('window:beforeprint')
  fitToOnePage(): void {
    const el = this.sheet?.nativeElement;
    if (!el) {
      return;
    }
    const pageWidthPx = 794;
    const pageHeightPx = 1122;
    el.style.removeProperty('zoom');
    el.classList.add('measuring');
    el.style.width = `${pageWidthPx}px`;
    const height = el.scrollHeight;
    el.style.removeProperty('width');
    el.classList.remove('measuring');
    if (height > pageHeightPx) {
      el.style.setProperty('zoom', String(Math.floor((pageHeightPx / height) * 1000) / 1000));
    }
  }

  @HostListener('window:afterprint')
  resetFit(): void {
    this.sheet?.nativeElement.style.removeProperty('zoom');
  }
}
