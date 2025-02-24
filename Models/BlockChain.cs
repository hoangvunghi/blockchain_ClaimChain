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
        // Lấy block cuối cùng trong chain
        var lastBlock = Blocks.LastOrDefault();
        
        // Tạo block mới với số thứ tự đúng
        var blockNumber = (lastBlock?.BlockNumber ?? -1) + 1;
        IBlock block = new Block(blockNumber);

        // Thêm các transaction vào block
        foreach (var transaction in transactions)
        {
            block.AddTransaction(transaction);
        }

        // Thiết lập Previous Hash từ block cuối cùng
        block.PreviousBlockHash = lastBlock?.BlockHash ?? "0";

        // Tính toán hash cho block mới
        block.SetBlockHash(lastBlock); // Truyền lastBlock thay vì CurrentBlock
        
        // Thêm block vào chain
        AcceptBlock(block);
    }

    public void AcceptBlock(IBlock block)
    {
        if (HeadBlock == null)
        {
            HeadBlock = block;
        }

        // Cập nhật liên kết NextBlock
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
}
}