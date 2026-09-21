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
import { MatTooltipModule } from '@angular/material/tooltip';
import { ItemService } from '../../../core/services/item.service';
import { Item } from '../../../core/models/models';
import { ItemCategory, ItemCategoryLabels } from '../../../core/models/enums';
import { ItemFormDialogComponent } from '../item-form-dialog/item-form-dialog.component';
import { StockAdjustDialogComponent } from '../stock-adjust-dialog/stock-adjust-dialog.component';

@Component({
  selector: 'app-item-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatDialogModule,
    MatTooltipModule
  ],
  templateUrl: './item-list.component.html',
  styleUrl: './item-list.component.scss'
})
export class ItemListComponent implements OnInit {
  items: Item[] = [];
  search = '';
  categoryLabels = ItemCategoryLabels;
  columns = ['name', 'category', 'hsn', 'unit', 'rate', 'gst', 'stock', 'actions'];

  constructor(
    private itemService: ItemService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.itemService.getAll(this.search || undefined).subscribe((data) => (this.items = data));
  }

  categoryLabel(category: ItemCategory): string {
    return this.categoryLabels[category];
  }

  openForm(item: Item | null): void {
    const ref = this.dialog.open(ItemFormDialogComponent, { width: '640px', data: { item } });
    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      const request = item ? this.itemService.update(item.id, result) : this.itemService.create(result);
      request.subscribe(() => {
        this.snackBar.open(item ? 'Item updated' : 'Item added', 'Close', { duration: 2500 });
        this.load();
      });
    });
  }

  adjustStock(item: Item): void {
    const ref = this.dialog.open(StockAdjustDialogComponent, { width: '480px', data: { item } });
    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.itemService.adjustStock(result).subscribe(() => {
        this.snackBar.open('Stock updated', 'Close', { duration: 2500 });
        this.load();
      });
    });
  }

  delete(item: Item): void {
    if (!confirm(`Delete item "${item.name}"?`)) return;
    this.itemService.delete(item.id).subscribe(() => {
      this.snackBar.open('Item removed', 'Close', { duration: 2500 });
      this.load();
    });
  }
}
