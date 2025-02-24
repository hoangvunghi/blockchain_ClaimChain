namespace BlockchainTest.Interfaces // Cùng namespace với ITransaction
{
    public interface IBlock
    {
        List<ITransaction> Transactions { get; set; }
        int BlockNumber { get; }  
        DateTime CreatedDate { get; set; }
        string BlockHash { get; set; }
        string PreviousBlockHash { get; set; }

        void AddTransaction(ITransaction transaction);
        string CalculateBlockHash(); 
        void SetBlockHash(IBlock parent);
        IBlock NextBlock { get; set; }
        bool IsValidChain(string previousBlockHash, bool verbose); 
    }
}