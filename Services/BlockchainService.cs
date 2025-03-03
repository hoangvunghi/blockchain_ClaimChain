using BlockchainTest.Models;
using BlockchainTest.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

public class BlockchainService
{
    private const int TRANSACTIONS_PER_BLOCK = 3;
    private readonly BlockChainContext _context;
    public BlockChain Blockchain { get; private set; }

    public BlockchainService(BlockChainContext context)
    {
        _context = context;
        InitializeBlockchain();
    }

    private void InitializeBlockchain()
    {
        // Đảm bảo database được tạo
        _context.Database.EnsureCreated();
        
        var hasBlocks = _context.Blocks.Any();
        
        if (!hasBlocks)
        {
            // Chỉ tạo mới nếu chưa có blocks
            Blockchain = new BlockChain();
            
            // Tạo genesis block với 3 transaction mẫu
            var genesisBlock = new Block(0)
            {
                BlockHash = "0000",
                PreviousBlockHash = "0",
                CreatedDate = DateTime.UtcNow
            };

            // Thêm 3 transaction mẫu vào genesis block
            var sampleTransactions = new List<ITransaction>
            {
                new Transaction
                {
                    ClaimNumber = "GENESIS-001",
                    SettlementAmount = 0,
                    SettlementDate = DateTime.UtcNow,
                    CarRegistration = "GENESIS",
                    Mileage = 0,
                    ClaimType = ClaimType.Collision
                },
                new Transaction
                {
                    ClaimNumber = "GENESIS-002",
                    SettlementAmount = 0,
                    SettlementDate = DateTime.UtcNow,
                    CarRegistration = "GENESIS",
                    Mileage = 0,
                    ClaimType = ClaimType.Theft
                },
                new Transaction
                {
                    ClaimNumber = "GENESIS-003",
                    SettlementAmount = 0,
                    SettlementDate = DateTime.UtcNow,
                    CarRegistration = "GENESIS",
                    Mileage = 0,
                    ClaimType = ClaimType.HailDamage
                }
            };

            foreach (var transaction in sampleTransactions)
            {
                genesisBlock.AddTransaction(transaction);
            }
            
            var blockModel = new BlockModel
            {
                BlockNumber = genesisBlock.BlockNumber,
                BlockHash = genesisBlock.BlockHash,
                PreviousBlockHash = genesisBlock.PreviousBlockHash,
                CreatedDate = genesisBlock.CreatedDate
            };

            foreach (var transaction in genesisBlock.Transactions)
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
            _context.SaveChanges();
            
            Blockchain.Blocks.Add(genesisBlock);
        }
        else
        {
            // Load dữ liệu từ database nếu đã có
            LoadBlockchainFromDatabase();
        }
    }

    public void AddTransaction(ITransaction transaction)
    {
        Blockchain.AddTransaction(transaction);
        SaveBlockchain();
    }

    public List<ITransaction> GetPendingTransactions()
    {
        return Blockchain.GetPendingTransactions();
    }

    public void SaveBlockchain()
    {
        // Lưu blocks mới
        foreach (var block in Blockchain.Blocks.Skip(_context.Blocks.Count()))
        {
            var blockModel = new BlockModel
            {
                BlockNumber = block.BlockNumber,
                BlockHash = block.BlockHash,
                PreviousBlockHash = block.PreviousBlockHash,
                CreatedDate = block.CreatedDate
            };

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

        // Lưu pending transactions
        _context.PendingTransactions.RemoveRange(_context.PendingTransactions);
        
        foreach (var transaction in GetPendingTransactions())
        {
            _context.PendingTransactions.Add(new PendingTransactionModel
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

        _context.SaveChanges();
    }

    private void LoadBlockchainFromDatabase()
    {
        Blockchain = new BlockChain();

        // Load blocks và transactions đã confirm
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

            Blockchain.Blocks.Add(block);
        }

        // Khôi phục liên kết NextBlock
        for (int i = 0; i < Blockchain.Blocks.Count - 1; i++)
        {
            Blockchain.Blocks[i].NextBlock = Blockchain.Blocks[i + 1];
        }

        // Load pending transactions
        var pendingTransactions = _context.PendingTransactions.ToList();

        foreach (var txn in pendingTransactions)
        {
            Blockchain.AddTransaction(new Transaction
            {
                ClaimNumber = txn.ClaimNumber,
                SettlementAmount = txn.SettlementAmount,
                SettlementDate = txn.SettlementDate,
                CarRegistration = txn.CarRegistration,
                Mileage = txn.Mileage,
                ClaimType = txn.ClaimType
            });
        }
    }
}