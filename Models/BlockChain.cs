namespace Quanlydiem.Models
{
    using Quanlydiem.Interfaces; 
    using System.Collections.Generic; 
    using System;
    using Quanlydiem.Services;
    
    public class BlockChain
    {
        public IBlock CurrentBlock { get; private set; }
        public IBlock HeadBlock { get; private set; }
        public List<IBlock> Blocks { get; } = new List<IBlock>();
        private List<ITransaction> pendingTransactions = new List<ITransaction>();
        private const int TRANSACTIONS_PER_BLOCK = 3;
        
        // Sự kiện để thông báo khi phát hiện block bị sửa đổi
        public event EventHandler<BlockIntegrityEventArgs> BlockIntegrityChanged;

        public void AddTransaction(ITransaction transaction)
        {
            // Kiểm tra tính toàn vẹn của blockchain trước khi thêm transaction mới
            VerifyBlockchainIntegrity();
            
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
        
        /// <summary>
        /// Kiểm tra tính toàn vẹn của blockchain bằng cách tính toán lại Merkle root
        /// </summary>
        public List<BlockIntegrityResult> VerifyBlockchainIntegrity()
        {
            var integrityService = new BlockchainIntegrityService(this);
            var results = integrityService.VerifyBlockchainIntegrity();
            
            // Nếu phát hiện có block bị sửa đổi, kích hoạt sự kiện
            if (results.Count > 0)
            {
                BlockIntegrityChanged?.Invoke(this, new BlockIntegrityEventArgs 
                { 
                    IntegrityResults = results 
                });
            }
            
            return results;
        }
    }
    
    /// <summary>
    /// Lớp chứa thông tin về sự kiện khi phát hiện blockchain bị sửa đổi
    /// </summary>
    public class BlockIntegrityEventArgs : EventArgs
    {
        public List<BlockIntegrityResult> IntegrityResults { get; set; } = new List<BlockIntegrityResult>();
    }
}