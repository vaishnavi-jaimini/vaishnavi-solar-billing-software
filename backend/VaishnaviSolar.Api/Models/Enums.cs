namespace VaishnaviSolar.Api.Models;

public enum ItemCategory
{
    Panel,
    Inverter,
    Battery,
    MountingStructure,
    CableWire,
    BalanceOfSystem,
    InstallationService,
    Other
}

public enum ItemUnit
{
    Nos,
    Watt,
    Set,
    Meter,
    Kg,
    Service
}

public enum StockTransactionType
{
    Opening,
    Purchase,
    Sale,
    Adjustment
}

public enum QuotationStatus
{
    Draft,
    Sent,
    Accepted,
    Rejected,
    Converted
}

public enum InvoiceStatus
{
    Unpaid,
    PartiallyPaid,
    Paid
}

public enum PaymentMode
{
    Cash,
    BankTransfer,
    UPI,
    Cheque,
    Card
}
