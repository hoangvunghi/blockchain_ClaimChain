namespace BlockchainTest.Models
{
    using BlockchainTest.Interfaces; 
    using System.Collections.Generic; 
    using System; 
    
    public class BlockChain
    {
        public IBlock CurrentBlock { get; private set; }
        public IBlock HeadBlock { get; private set; }
        public List<IBlock> Blocks { get; } = new List<IBlock>();
        private List<ITransaction> pendingTransactions = new List<ITransaction>();
        private const int TRANSACTIONS_PER_BLOCK = 3;

        public void AddTransaction(ITransaction transaction)
        {
            pendingTransactions.Add(transaction);
            
            if (pendingTransactions.Count >= TRANSACTIONS_PER_BLOCK)
            {
                AddBlock(new List<ITransaction>(pendingTransactions));
                pendingTransactions.Clear();
            }
        }

        private void AddBlock(List<ITransaction> transactions)
        {
            var lastBlock = Blocks.LastOrDefault();
            var blockNumber = (lastBlock?.BlockNumber ?? -1) + 1;
            IBlock block = new Block(blockNumber);

            foreach (var transaction in transactions)
            {
                block.AddTransaction(transaction);
            }

            block.PreviousBlockHash = lastBlock?.BlockHash ?? "0";
            block.SetBlockHash(lastBlock);
            AcceptBlock(block);
        }

        public void AcceptBlock(IBlock block)
        {
            if (HeadBlock == null)
            {
                HeadBlock = block;
            }

            if (CurrentBlock != null)
            {
                CurrentBlock.NextBlock = block;
            }

            CurrentBlock = block;
            Blocks.Add(block);
        }

        public void VerifyChain()
        {
            if (HeadBlock == null)
            {
                throw new InvalidOperationException("No blocks in the chain to verify");
            }
            bool isValid = HeadBlock.IsValidChain(null, true);

            if (isValid)
            {
                Console.WriteLine("Blockchain is valid");
            }
            else
            {
                Console.WriteLine("Blockchain is invalid");
            }
        }

        public List<ITransaction> GetPendingTransactions()
        {
            return new List<ITransaction>(pendingTransactions);
        }
    }
}