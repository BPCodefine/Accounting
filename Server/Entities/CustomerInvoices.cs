namespace AccountingServer.Entities
{
    public class CustomerInvoices
    {
        public required string InvoiceNo { get; set; }
        public required string CustomerNo { get; set; }
        public required string CustomerName { get; set; }
        public required string Description { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public required string Cur { get; set; }
        public decimal AmountLCY { get; set; }
        public DateTime PaymentDate { get; set; }
        public required string PaymentDocNo { get; set; }
        public int LateDays { get; set; }
    }
}
