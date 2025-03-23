using Quanlydiem.Interfaces;
using System.Collections.Generic;

namespace Quanlydiem.Models
{
    public class BlockchainViewModel
    {
        public BlockChain Blockchain { get; set; }
        public List<ITransaction> PendingTransactions { get; set; }
        public bool IsBlockchainValid { get; set; }
    }
} 