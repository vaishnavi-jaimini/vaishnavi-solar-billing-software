import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent)
  },
  {
    path: 'customers',
    loadComponent: () =>
      import('./features/customers/customer-list/customer-list.component').then((m) => m.CustomerListComponent)
  },
  {
    path: 'items',
    loadComponent: () => import('./features/items/item-list/item-list.component').then((m) => m.ItemListComponent)
  },
  {
    path: 'quotations',
    loadComponent: () =>
      import('./features/quotations/quotation-list/quotation-list.component').then((m) => m.QuotationListComponent)
  },
  {
    path: 'quotations/new',
    loadComponent: () =>
      import('./features/quotations/quotation-form/quotation-form.component').then((m) => m.QuotationFormComponent)
  },
  {
    path: 'quotations/:id',
    loadComponent: () =>
      import('./features/quotations/quotation-form/quotation-form.component').then((m) => m.QuotationFormComponent)
  },
  {
    path: 'invoices',
    loadComponent: () =>
      import('./features/invoices/invoice-list/invoice-list.component').then((m) => m.InvoiceListComponent)
  },
  {
    path: 'invoices/new',
    loadComponent: () =>
      import('./features/invoices/invoice-form/invoice-form.component').then((m) => m.InvoiceFormComponent)
  },
  {
    path: 'invoices/:id',
    loadComponent: () =>
      import('./features/invoices/invoice-form/invoice-form.component').then((m) => m.InvoiceFormComponent)
  },
  {
    path: 'invoices/:id/print',
    loadComponent: () =>
      import('./features/invoices/invoice-print/invoice-print.component').then((m) => m.InvoicePrintComponent)
  },
  {
    path: 'proforma-invoices',
    loadComponent: () =>
      import('./features/proforma-invoices/proforma-invoice-list/proforma-invoice-list.component').then(
        (m) => m.ProformaInvoiceListComponent
      )
  },
  {
    path: 'proforma-invoices/new',
    loadComponent: () =>
      import('./features/proforma-invoices/proforma-invoice-form/proforma-invoice-form.component').then(
        (m) => m.ProformaInvoiceFormComponent
      )
  },
  {
    path: 'proforma-invoices/:id',
    loadComponent: () =>
      import('./features/proforma-invoices/proforma-invoice-form/proforma-invoice-form.component').then(
        (m) => m.ProformaInvoiceFormComponent
      )
  },
  {
    path: 'proforma-invoices/:id/print',
    loadComponent: () =>
      import('./features/proforma-invoices/proforma-invoice-print/proforma-invoice-print.component').then(
        (m) => m.ProformaInvoicePrintComponent
      )
  },
  {
    path: 'payments',
    loadComponent: () =>
      import('./features/payments/payment-list/payment-list.component').then((m) => m.PaymentListComponent)
  },
  {
    path: 'reports',
    loadComponent: () => import('./features/reports/reports/reports.component').then((m) => m.ReportsComponent)
  },
  {
    path: 'settings',
    loadComponent: () => import('./features/settings/settings/settings.component').then((m) => m.SettingsComponent)
  },
  { path: '**', redirectTo: 'dashboard' }
];
