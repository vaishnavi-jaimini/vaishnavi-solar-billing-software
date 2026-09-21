export enum ItemCategory {
  Panel = 'Panel',
  Inverter = 'Inverter',
  Battery = 'Battery',
  MountingStructure = 'MountingStructure',
  CableWire = 'CableWire',
  BalanceOfSystem = 'BalanceOfSystem',
  InstallationService = 'InstallationService',
  Other = 'Other'
}

export const ItemCategoryLabels: Record<ItemCategory, string> = {
  [ItemCategory.Panel]: 'Solar Panel',
  [ItemCategory.Inverter]: 'Inverter',
  [ItemCategory.Battery]: 'Battery',
  [ItemCategory.MountingStructure]: 'Mounting Structure',
  [ItemCategory.CableWire]: 'Cable / Wire',
  [ItemCategory.BalanceOfSystem]: 'Balance of System',
  [ItemCategory.InstallationService]: 'Installation / Service',
  [ItemCategory.Other]: 'Other'
};

export enum ItemUnit {
  Nos = 'Nos',
  Watt = 'Watt',
  Set = 'Set',
  Meter = 'Meter',
  Kg = 'Kg',
  Service = 'Service'
}

export enum StockTransactionType {
  Opening = 'Opening',
  Purchase = 'Purchase',
  Sale = 'Sale',
  Adjustment = 'Adjustment'
}

export enum QuotationStatus {
  Draft = 'Draft',
  Sent = 'Sent',
  Accepted = 'Accepted',
  Rejected = 'Rejected',
  Converted = 'Converted'
}

export enum InvoiceStatus {
  Unpaid = 'Unpaid',
  PartiallyPaid = 'PartiallyPaid',
  Paid = 'Paid'
}

export enum PaymentMode {
  Cash = 'Cash',
  BankTransfer = 'BankTransfer',
  UPI = 'UPI',
  Cheque = 'Cheque',
  Card = 'Card'
}

export const INDIAN_STATES: string[] = [
  'Andhra Pradesh', 'Arunachal Pradesh', 'Assam', 'Bihar', 'Chhattisgarh', 'Goa', 'Gujarat',
  'Haryana', 'Himachal Pradesh', 'Jharkhand', 'Karnataka', 'Kerala', 'Madhya Pradesh',
  'Maharashtra', 'Manipur', 'Meghalaya', 'Mizoram', 'Nagaland', 'Odisha', 'Punjab',
  'Rajasthan', 'Sikkim', 'Tamil Nadu', 'Telangana', 'Tripura', 'Uttar Pradesh',
  'Uttarakhand', 'West Bengal', 'Andaman and Nicobar Islands', 'Chandigarh',
  'Dadra and Nagar Haveli and Daman and Diu', 'Delhi', 'Jammu and Kashmir', 'Ladakh',
  'Lakshadweep', 'Puducherry'
];
