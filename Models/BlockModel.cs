namespace BlockchainTest.Models
{
    public class BlockModel
    {
        public int Id { get; set; } // Khóa chính
        public int BlockNumber { get; set; }
        public string BlockHash { get; set; }
        public string? PreviousBlockHash { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? NextBlockId { get; set; } // Tham chiếu đến khối tiếp theo (nullable nếu là khối cuối)
        public int? BlockChainId { get; set; } // Tham chiếu đến BlockChain
        public BlockModel NextBlock { get; set; }
        // Quan hệ với Transaction
        public List<TransactionModel> Transactions { get; set; } = new List<TransactionModel>();
    }
}