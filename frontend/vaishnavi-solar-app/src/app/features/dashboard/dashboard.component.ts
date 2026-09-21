import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { ReportService } from '../../core/services/report.service';
import { Dashboard } from '../../core/models/models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, MatCardModule, MatIconModule, MatButtonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  data: Dashboard | null = null;
  loading = true;
  hoverIndex: number | null = null;

  constructor(private reportService: ReportService) {}

  ngOnInit(): void {
    this.reportService.dashboard().subscribe((d) => {
      this.data = d;
      this.loading = false;
    });
  }

  get maxTrend(): number {
    if (!this.data || this.data.salesTrend.length === 0) return 1;
    return Math.max(...this.data.salesTrend.map((p) => p.total), 1);
  }

  barHeight(total: number): number {
    return this.maxTrend > 0 ? Math.round((total / this.maxTrend) * 140) : 0;
  }

  formatInr(value: number): string {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      maximumFractionDigits: 0
    }).format(value || 0);
  }
}
