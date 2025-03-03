using BlockchainTest.Interfaces; 
using BlockchainTest;
using System;
using System.Text;
namespace BlockchainTest.Models
{
    using System;

    public class PendingTransactionModel: ITransaction
    {
        public int Id { get; set; }
        public string ClaimNumber { get; set; }
        public decimal SettlementAmount { get; set; }
        public DateTime SettlementDate { get; set; }
        public string CarRegistration { get; set; }
        public int Mileage { get; set; }
        public ClaimType ClaimType { get; set; }
        public string TransactionHash { get; set; }

        public string CalculateTransactionHash()
        {
            string transactionData = System.Text.Json.JsonSerializer.Serialize(new
            {
                ClaimNumber,
                SettlementAmount,
                SettlementDate,
                CarRegistration,
                Mileage,
                ClaimType
            });
            return Convert.ToBase64String(Hashing.ComputeHashSha256(Encoding.UTF8.GetBytes(transactionData)));
        }
    }
}
