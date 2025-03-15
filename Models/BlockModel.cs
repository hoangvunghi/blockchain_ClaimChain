namespace Quanlydiem.Models
{
    public class BlockModel
    {
        public int Id { get; set; } // Khóa chính
        public int BlockNumber { get; set; }
        public string BlockHash { get; set; }
        public string? PreviousBlockHash { get; set; }
        public string MerkleRoot { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? NextBlockId { get; set; }
        public int? BlockChainId { get; set; } 
        public BlockModel NextBlock { get; set; }

        public List<TransactionModel> Transactions { get; set; } = new List<TransactionModel>();
    }
}