using BlockchainTest.Models;
using BlockchainTest.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

public class BlockchainService
{
    private readonly BlockChainContext _context;
    public BlockChain Blockchain { get; private set; }

    public BlockchainService(BlockChainContext context)
    {
        _context = context;
        InitializeBlockchain();
    }

    private void InitializeBlockchain()
{
    var hasBlocks = _context.Blocks.Any();
    
    if (!hasBlocks)
    {
        Blockchain = new BlockChain();
        InitializeSampleData();
        SaveBlockchain();
    }
    else
    {
        LoadBlockchainFromDatabase();
    }
}


    private void InitializeSampleData()
{
    // Tạo block đầu tiên (Genesis Block)
    var genesisBlock = new Block(0)
    {
        BlockHash = "0000",
        PreviousBlockHash = "0",  // ✅ Đảm bảo không NULL
        CreatedDate = DateTime.UtcNow
    };
    Blockchain.Blocks.Add(genesisBlock);

    // Thêm giao dịch mẫu
    var transaction1 = new Transaction
    {
        ClaimNumber = "Claim001",
        SettlementAmount = 1500.00m,
        SettlementDate = DateTime.UtcNow,
        CarRegistration = "CAR-123",
        Mileage = 5000,
        ClaimType = ClaimType.Collision
    };
    Blockchain.AddBlock(new List<ITransaction> { transaction1 });

    var transaction2 = new Transaction
    {
        ClaimNumber = "Claim002",
        SettlementAmount = 2500.00m,
        SettlementDate = DateTime.UtcNow,
        CarRegistration = "CAR-456",
        Mileage = 12000,
        ClaimType = ClaimType.Theft
    };
    Blockchain.AddBlock(new List<ITransaction> { transaction2 });
}


public void SaveBlockchain()
{
    // First, clear existing blocks to prevent duplicates
    if (_context.Blocks.Any())
    {
        _context.Blocks.RemoveRange(_context.Blocks);
        _context.SaveChanges();
    }

    foreach (var block in Blockchain.Blocks)
    {
        var blockModel = new BlockModel
        {
            BlockNumber = block.BlockNumber,
            BlockHash = block.BlockHash,
            PreviousBlockHash = block.BlockNumber == 0 ? "0" : block.PreviousBlockHash ?? "0",
            CreatedDate = block.CreatedDate
        };

        // Lưu giao dịch
        foreach (var transaction in block.Transactions)
        {
            blockModel.Transactions.Add(new TransactionModel
            {
                ClaimNumber = transaction.ClaimNumber,
                SettlementAmount = transaction.SettlementAmount,
                SettlementDate = transaction.SettlementDate,
                CarRegistration = transaction.CarRegistration,
                Mileage = transaction.Mileage,
                ClaimType = transaction.ClaimType,
                TransactionHash = transaction.CalculateTransactionHash()
            });
        }

        _context.Blocks.Add(blockModel);
    }

    _context.SaveChanges();
}

    private void LoadBlockchainFromDatabase()
{
    Blockchain = new BlockChain();

    var blocks = _context.Blocks
        .Include(b => b.Transactions)
        .OrderBy(b => b.BlockNumber)
        .ToList();

    foreach (var blockModel in blocks)
    {
        var block = new Block(blockModel.BlockNumber)
        {
            BlockHash = blockModel.BlockHash,
            PreviousBlockHash = blockModel.PreviousBlockHash,
            CreatedDate = blockModel.CreatedDate
        };

        foreach (var transactionModel in blockModel.Transactions)
        {
            block.AddTransaction(new Transaction
            {
                ClaimNumber = transactionModel.ClaimNumber,
                SettlementAmount = transactionModel.SettlementAmount,
                SettlementDate = transactionModel.SettlementDate,
                CarRegistration = transactionModel.CarRegistration,
                Mileage = transactionModel.Mileage,
                ClaimType = transactionModel.ClaimType
            });
        }

        // Chỉ thêm block một lần:
        Blockchain.Blocks.Add(block);
    }

    // Khôi phục liên kết NextBlock
    for (int i = 0; i < blocks.Count - 1; i++)
    {
        Blockchain.Blocks[i].NextBlock = Blockchain.Blocks[i + 1];
    }
}

    public void AddBlock(List<ITransaction> transactions)
    {
        // Tạo và thêm khối mới
        Blockchain.AddBlock(transactions);
        SaveBlockchain();
    }
}