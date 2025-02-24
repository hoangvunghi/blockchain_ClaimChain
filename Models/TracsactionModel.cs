using BlockchainTest.Interfaces;
namespace BlockchainTest.Models
{
    public class TransactionModel
    {
        public int Id { get; set; } // Khóa chính
        public string ClaimNumber { get; set; }
        public decimal SettlementAmount { get; set; }
        public DateTime SettlementDate { get; set; }
        public string CarRegistration { get; set; }
        public int Mileage { get; set; }
        public ClaimType ClaimType { get; set; }
        public string TransactionHash { get; set; } // Hash của transaction
        public int BlockId { get; set; } // Tham chiếu đến Block
        public BlockModel Block { get; set; } // Quan hệ ngược
    }
}
