import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DeliveryChallanService } from '../../../core/services/delivery-challan.service';
import { DeliveryChallan } from '../../../core/models/models';

@Component({
  selector: 'app-delivery-challan-list',
  standalone: true,
  imports: [CommonModule, RouterLink, MatTableModule, MatButtonModule, MatIconModule, MatTooltipModule],
  templateUrl: './delivery-challan-list.component.html',
  styleUrl: './delivery-challan-list.component.scss'
})
export class DeliveryChallanListComponent implements OnInit {
  challans: DeliveryChallan[] = [];
  columns = ['challanNumber', 'date', 'customerName', 'vehicleNumber', 'grandTotal', 'actions'];

  constructor(
    private challanService: DeliveryChallanService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.challanService.getAll().subscribe((data) => (this.challans = data));
  }

  delete(c: DeliveryChallan): void {
    if (!confirm(`Delete delivery challan ${c.challanNumber}?`)) return;
    this.challanService.delete(c.id).subscribe({
      next: () => this.load(),
      error: (err) => this.snackBar.open(err.error || 'Could not delete delivery challan', 'Close', { duration: 3000 })
    });
  }
}
