using System;
using TryingOurBest1.Helpers;

namespace TryingOurBest1.Models
{
    public class Transaction
    {
        public int TransactionID { get; set; }
        public int BuyerID { get; set; }
        public int? SellerID { get; set; }
        public int ItemID { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public DateTime Timestamp { get; set; }

        // --- Computed display properties for UI binding ---
        public string DisplayType => TransactionTypeMapper.ToDisplayString(Type);
        public string DisplayStatus => TransactionTypeMapper.StatusToDisplayString(Status);
        public string DisplayAmount => TransactionTypeMapper.FormatAmount(Amount);
        public string DisplayTimestamp => Timestamp.ToString("dd/MM/yyyy HH:mm");
    }
}