import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { DeliveryChallanService } from '../../../core/services/delivery-challan.service';
import { CompanyService } from '../../../core/services/company.service';
import { Company, DeliveryChallan } from '../../../core/models/models';
import { gstRateRows, GstRateRow } from '../../../core/utils/gst-breakup';

@Component({
  selector: 'app-delivery-challan-print',
  standalone: true,
  imports: [CommonModule, RouterLink, MatButtonModule, MatCheckboxModule, MatIconModule],
  templateUrl: './delivery-challan-print.component.html',
  styleUrl: './delivery-challan-print.component.scss'
})
export class DeliveryChallanPrintComponent implements OnInit {
  challan: DeliveryChallan | null = null;
  company: Company | null = null;
  showValues = true;
  // A challan is issued in triplicate, so each print produces all three copies.
  copies = ['Original for Consignee', 'Duplicate for Transporter', 'Triplicate for Consignor'];

  constructor(
    private route: ActivatedRoute,
    private challanService: DeliveryChallanService,
    private companyService: CompanyService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.challanService.getById(id).subscribe((c) => (this.challan = c));
    this.companyService.get().subscribe((c) => (this.company = c));
  }

  gstRows(doc: DeliveryChallan): GstRateRow[] {
    return gstRateRows(doc.items, doc.isInterState);
  }

  print(): void {
    window.print();
  }
}
