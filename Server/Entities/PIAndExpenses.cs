namespace AccountingServer.Entities
{
    public class PIAndExpenses
    {
        public required string InvoiceNo { get; set; }
        public required string BuyFromID { get; set; }
        public required string BuyFromName { get; set; }
        public required string PayToID { get; set; }
        public required string PayToName { get; set; }
        public string? YourRefOrderID { get; set; }
        public DateTime PostingDate { get; set; }
        public required string InvLineNo { get; set; }
        public string? ArticleNo { get; set; }
        public string? ArticleName { get; set; }
        public decimal InvoicedQty { get; set; }
        public decimal PUACost { get; set; }
        public decimal SeafreightCost { get; set; }
        public decimal DutyCost { get; set; }
        public decimal StorageCost { get; set; }
        public decimal OutfreightCost { get; set; }
        public decimal InlandFreightCost { get; set; }
        public decimal SmallCost { get; set; }
        public decimal UVYarnCost { get; set; }
        public decimal QualDiscountCost { get; set; }
    }
}
