import { InvoiceStatus, ItemCategory, ItemUnit, PaymentMode, QuotationStatus } from './enums';

export interface Company {
  id: number;
  name: string;
  address: string;
  city: string;
  state: string;
  pincode: string;
  gstin: string;
  phone: string;
  email: string;
  bankName: string;
  bankAccountNo: string;
  bankIfsc: string;
  bankAccountName: string;
  bankBranch: string;
  invoicePrefix: string;
}

export interface Customer {
  id: number;
  name: string;
  address: string;
  city: string;
  state: string;
  pincode: string;
  phone: string;
  email: string;
  gstin?: string | null;
}

export type CustomerUpsert = Omit<Customer, 'id'>;

export interface Item {
  id: number;
  name: string;
  category: ItemCategory;
  hsnSacCode: string;
  unit: ItemUnit;
  rate: number;
  gstPercent: number;
  currentStock: number;
  lowStockThreshold: number;
  isService: boolean;
  isActive: boolean;
}

export type ItemUpsert = Omit<Item, 'id' | 'currentStock'>;

export interface StockAdjustment {
  itemId: number;
  type: string;
  quantity: number;
  notes?: string;
}

export interface QuotationItem {
  id: number;
  itemId: number;
  itemName: string;
  description: string;
  quantity: number;
  rate: number;
  gstPercent: number;
  amount: number;
}

export interface Quotation {
  id: number;
  quotationNumber: string;
  date: string;
  validUntil?: string | null;
  customerId: number;
  customerName: string;
  status: QuotationStatus;
  subtotal: number;
  totalGst: number;
  grandTotal: number;
  notes?: string;
  convertedInvoiceId?: number | null;
  items: QuotationItem[];
}

export interface LineItemInput {
  itemId: number;
  description?: string;
  quantity: number;
  rate: number;
  gstPercent: number;
}

export interface QuotationUpsert {
  customerId: number;
  date: string;
  validUntil?: string | null;
  notes?: string;
  items: LineItemInput[];
}

export interface InvoiceItem {
  id: number;
  itemId: number;
  itemName: string;
  description: string;
  hsnSacCode: string;
  quantity: number;
  unit: ItemUnit;
  rate: number;
  gstPercent: number;
  cgstAmount: number;
  sgstAmount: number;
  igstAmount: number;
  amount: number;
}

export interface Invoice {
  id: number;
  invoiceNumber: string;
  invoiceDate: string;
  customerId: number;
  customerName: string;
  quotationId?: number | null;
  placeOfSupply: string;
  isInterState: boolean;
  shipToName: string;
  shipToAddress: string;
  shipToCity: string;
  shipToState: string;
  shipToPincode: string;
  terms: string;
  dueDate: string;
  workOrderNo: string;
  purchaseOrderNo: string;
  purchaseDate?: string | null;
  subtotal: number;
  cgstAmount: number;
  sgstAmount: number;
  igstAmount: number;
  totalGst: number;
  grandTotal: number;
  amountPaid: number;
  balance: number;
  status: InvoiceStatus;
  notes?: string;
  items: InvoiceItem[];
}

export interface InvoiceUpsert {
  customerId: number;
  invoiceDate: string;
  notes?: string;
  shipSameAsBilling: boolean;
  shipToName?: string;
  shipToAddress?: string;
  shipToCity?: string;
  shipToState?: string;
  shipToPincode?: string;
  placeOfSupply?: string;
  terms?: string;
  dueDate?: string;
  workOrderNo?: string;
  purchaseOrderNo?: string;
  purchaseDate?: string | null;
  items: LineItemInput[];
}

export interface ProformaInvoiceItem {
  id: number;
  itemId: number;
  itemName: string;
  description: string;
  hsnSacCode: string;
  quantity: number;
  unit: ItemUnit;
  rate: number;
  gstPercent: number;
  cgstAmount: number;
  sgstAmount: number;
  igstAmount: number;
  amount: number;
}

export interface ProformaInvoice {
  id: number;
  proformaNumber: string;
  proformaDate: string;
  customerId: number;
  customerName: string;
  placeOfSupply: string;
  isInterState: boolean;
  shipToName: string;
  shipToAddress: string;
  shipToCity: string;
  shipToState: string;
  shipToPincode: string;
  subtotal: number;
  cgstAmount: number;
  sgstAmount: number;
  igstAmount: number;
  totalGst: number;
  grandTotal: number;
  notes?: string;
  convertedInvoiceId?: number | null;
  items: ProformaInvoiceItem[];
}

export interface ProformaInvoiceUpsert {
  customerId: number;
  proformaDate: string;
  notes?: string;
  shipSameAsBilling: boolean;
  shipToName?: string;
  shipToAddress?: string;
  shipToCity?: string;
  shipToState?: string;
  shipToPincode?: string;
  items: LineItemInput[];
}

export interface Payment {
  id: number;
  invoiceId: number;
  invoiceNumber: string;
  customerName: string;
  paymentDate: string;
  amount: number;
  mode: PaymentMode;
  referenceNo?: string;
  notes?: string;
}

export interface PaymentUpsert {
  invoiceId: number;
  paymentDate: string;
  amount: number;
  mode: PaymentMode;
  referenceNo?: string;
  notes?: string;
}

export interface SalesReportRow {
  invoiceId: number;
  invoiceNumber: string;
  invoiceDate: string;
  customerName: string;
  subtotal: number;
  totalGst: number;
  grandTotal: number;
  status: string;
}

export interface SalesReport {
  rows: SalesReportRow[];
  totalSales: number;
  totalGstCollected: number;
  invoiceCount: number;
}

export interface GstSummaryRow {
  hsnSacCode: string;
  itemName: string;
  taxableValue: number;
  cgstAmount: number;
  sgstAmount: number;
  igstAmount: number;
  totalTax: number;
}

export interface GstSummary {
  rows: GstSummaryRow[];
  totalTaxableValue: number;
  totalCgst: number;
  totalSgst: number;
  totalIgst: number;
}

export interface OutstandingDueRow {
  invoiceId: number;
  invoiceNumber: string;
  invoiceDate: string;
  customerName: string;
  customerPhone: string;
  grandTotal: number;
  amountPaid: number;
  balance: number;
}

export interface StockReportRow {
  itemId: number;
  itemName: string;
  category: string;
  currentStock: number;
  lowStockThreshold: number;
  isLow: boolean;
}

export interface SalesTrendPoint {
  label: string;
  total: number;
}

export interface Dashboard {
  salesThisMonth: number;
  outstandingDues: number;
  lowStockCount: number;
  invoiceCountThisMonth: number;
  salesTrend: SalesTrendPoint[];
}
