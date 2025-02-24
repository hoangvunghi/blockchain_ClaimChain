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

        public void AddBlock(List<ITransaction> transactions) 
        {
            IBlock block = new Block(Blocks.Count + 1); 

            foreach (var transaction in transactions)
            {
                block.AddTransaction(transaction);
            }
            block.SetBlockHash(CurrentBlock); 
            AcceptBlock(block);  
        }

        public void AcceptBlock(IBlock block) 
        {
            if (HeadBlock == null)
            {
                HeadBlock = block;
                HeadBlock.PreviousBlockHash = null;
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
    }
}