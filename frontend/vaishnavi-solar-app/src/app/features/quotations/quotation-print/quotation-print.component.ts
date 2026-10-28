import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { QuotationService } from '../../../core/services/quotation.service';
import { CompanyService } from '../../../core/services/company.service';
import { Company, Quotation } from '../../../core/models/models';
import { numberToIndianWords } from '../../../core/utils/number-to-words';
import { DEFAULT_QUOTATION_TERMS } from '../quotation-terms';

@Component({
  selector: 'app-quotation-print',
  standalone: true,
  imports: [CommonModule, RouterLink, MatButtonModule, MatIconModule],
  templateUrl: './quotation-print.component.html',
  styleUrl: './quotation-print.component.scss'
})
export class QuotationPrintComponent implements OnInit {
  quotation: Quotation | null = null;
  company: Company | null = null;

  constructor(
    private route: ActivatedRoute,
    private quotationService: QuotationService,
    private companyService: CompanyService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.quotationService.getById(id).subscribe((q) => (this.quotation = q));
    this.companyService.get().subscribe((c) => (this.company = c));
  }

  amountInWords(amount: number): string {
    return numberToIndianWords(amount);
  }

  // Quotations saved before terms were editable have none stored, so they print the standard list.
  termLines(quotation: Quotation): string[] {
    const terms = quotation.termsAndConditions?.trim() || DEFAULT_QUOTATION_TERMS;
    return terms.split('\n').map((t) => t.trim()).filter((t) => t);
  }

  print(): void {
    window.print();
  }
}
