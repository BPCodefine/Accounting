using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace AccountingServer.Entities
{
    public class BankAccountLedgerEntries
    {
        public required string EntryNo { get; set; }
        public required string DocumentType { get; set; }
        public required string DocumentNo { get; set; }
        public required string BankAccountNo { get; set; }
        public required string Description { get; set; }
        public required DateTime PostingDate { get; set; }
        public required string CurrencyCode { get; set; }
        public required decimal Amount { get; set; }
        public required decimal RemainingAmount { get; set; }
        public required decimal AmountLCY { get; set; }
        public required bool Open { get; set; }
        public required decimal DebitAmount { get; set; }
        public required decimal CreditAmount { get; set; }
        public required decimal RunningTotalAmount { get; set; }
        public required decimal RunningTotalAmountLCY { get; set; }
    }
}
