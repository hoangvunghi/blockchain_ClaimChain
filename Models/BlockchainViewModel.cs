using BlockchainTest.Interfaces;
using System.Collections.Generic;

namespace BlockchainTest.Models
{
    public class BlockchainViewModel
    {
        public BlockChain Blockchain { get; set; }
        public List<ITransaction> PendingTransactions { get; set; }
    }
} 