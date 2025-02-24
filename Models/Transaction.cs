namespace BlockchainTest.Models 
{
    using System.Security.Cryptography;
    // import để sử dụng convert
    using System;
    using System.Text;
    using BlockchainTest;
    using BlockchainTest.Interfaces; 
    public class Transaction : ITransaction
    {
        public string ClaimNumber { get; set; }
        public decimal SettlementAmount { get; set; }
        public DateTime SettlementDate { get; set; }
        public string CarRegistration { get; set; }
        public int Mileage { get; set; }
        public ClaimType ClaimType { get; set; }

        public string CalculateTransactionHash()
        {
            string txnData = ClaimNumber + SettlementAmount + SettlementDate + CarRegistration + Mileage + ClaimType;
            return Convert.ToBase64String(Hashing.ComputeHashSha256(Encoding.UTF8.GetBytes(txnData)));
        }
    }
}