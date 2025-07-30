namespace AccountingServer.Entities
{
    public class CustAgedInvoices
    {
        public required string InvoiceNo { get; set; }
        public required string CustomerNo { get; set; }
        public required string CustomerName { get; set; }
        public DateTime InvoiceDate { get; set; }
        public required string Descr { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public required string Cur { get; set; }   
        public int LateDays { get; set; }
        public decimal Acc30 { get; set; }
        public decimal Acc3060 { get; set; }
        public decimal Acc6090 { get; set; }
        public decimal Acc90 { get; set; }
        public decimal LCY30 { get; set; }
        public decimal LCY3060 { get; set; }
        public decimal LCY6090 { get; set; }
        public decimal LCY90 { get; set; }
    }
}
