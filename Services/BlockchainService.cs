using Quanlydiem.Models;
using Quanlydiem.Interfaces;
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
                    masinhvien = "A45156",
                    mamonhoc = "INT1001",
                    diem = 8,
                    ngayluudiem = DateTime.UtcNow,
                    diemlanthu = 1
                },
                new Transaction
                {
                    masinhvien = "A45157",
                    mamonhoc = "INT1001",
                    diem = 9,
                    ngayluudiem = DateTime.UtcNow,
                    diemlanthu = 1
                },
                new Transaction
                {
                    masinhvien = "A45158",
                    mamonhoc = "INT1001",
                    diem = 10,
                    ngayluudiem = DateTime.UtcNow,
                    diemlanthu = 1
                }
            };

            foreach (var transaction in sampleTransactions)
            {
                genesisBlock.AddTransaction(transaction);
            }
            
            // Đảm bảo MerkleRoot được tính toán
            genesisBlock.MerkleRoot = genesisBlock.CalculateMerkleRoot();
            
            var blockModel = new BlockModel
            {
                BlockNumber = genesisBlock.BlockNumber,
                BlockHash = genesisBlock.BlockHash,
                PreviousBlockHash = genesisBlock.PreviousBlockHash,
                MerkleRoot = genesisBlock.MerkleRoot,
                CreatedDate = genesisBlock.CreatedDate
            };

            foreach (var transaction in genesisBlock.Transactions)
            {
                blockModel.Transactions.Add(new TransactionModel
                {
                    masinhvien = transaction.masinhvien,
                    mamonhoc = transaction.mamonhoc,
                    diem = transaction.diem,
                    ngayluudiem = transaction.ngayluudiem,
                    diemlanthu = transaction.diemlanthu,
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
        try
        {
            // Lưu blocks mới
            foreach (var block in Blockchain.Blocks.Skip(_context.Blocks.Count()))
            {
                var blockModel = new BlockModel
                {
                    BlockNumber = block.BlockNumber,
                    BlockHash = block.BlockHash,
                    PreviousBlockHash = block.PreviousBlockHash,
                    MerkleRoot = block.MerkleRoot,
                    CreatedDate = block.CreatedDate
                };

                foreach (var transaction in block.Transactions)
                {
                    blockModel.Transactions.Add(new TransactionModel
                    {
                        masinhvien = transaction.masinhvien,
                        mamonhoc = transaction.mamonhoc,
                        diem = transaction.diem,
                        ngayluudiem = transaction.ngayluudiem,
                        diemlanthu = transaction.diemlanthu,
                        TransactionHash = transaction.CalculateTransactionHash()
                    });
                }

                _context.Blocks.Add(blockModel);
            }

            // Xóa tất cả pending transactions cũ
            _context.PendingTransactions.RemoveRange(_context.PendingTransactions);
            
            // Lưu pending transactions mới
            var pendingTransactions = GetPendingTransactions();
            if (pendingTransactions.Any())
            {
                foreach (var transaction in pendingTransactions)
                {
                    var pendingTxn = new PendingTransactionModel
                    {
                        masinhvien = transaction.masinhvien,
                        mamonhoc = transaction.mamonhoc,
                        diem = transaction.diem,
                        ngayluudiem = transaction.ngayluudiem,
                        diemlanthu = transaction.diemlanthu,
                        TransactionHash = transaction.CalculateTransactionHash()
                    };
                    _context.PendingTransactions.Add(pendingTxn);
                }
            }

            // Lưu tất cả thay đổi vào database
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            // Log lỗi nếu có
            throw new Exception($"Error saving blockchain: {ex.Message}", ex);
        }
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
                PreviousBlockHash = blockModel.PreviousBlockHash ?? string.Empty,
                MerkleRoot = blockModel.MerkleRoot ?? string.Empty,
                CreatedDate = blockModel.CreatedDate
            };

            foreach (var transactionModel in blockModel.Transactions)
            {
                block.AddTransaction(new Transaction
                {
                    masinhvien = transactionModel.masinhvien,
                    mamonhoc = transactionModel.mamonhoc,
                    diem = transactionModel.diem,
                    ngayluudiem = transactionModel.ngayluudiem,
                    diemlanthu = transactionModel.diemlanthu
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
                masinhvien = txn.masinhvien,
                mamonhoc = txn.mamonhoc,
                diem = txn.diem,
                ngayluudiem = txn.ngayluudiem,
                diemlanthu = txn.diemlanthu
            });
        }
    }

    public bool VerifyBlockchainIntegrity()
    {
        try
        {
            if (Blockchain == null || Blockchain.Blocks.Count == 0)
            {
                return true; // Không có blocks để kiểm tra
            }

            // Kiểm tra tính toàn vẹn của chuỗi khối
            bool isChainValid = Blockchain.Blocks[0].IsValidChain(string.Empty, false);
            if (!isChainValid)
            {
                return false;
            }

            // Kiểm tra tính toàn vẹn của từng block trong database
            foreach (var block in Blockchain.Blocks)
            {
                // Lấy block từ database
                var dbBlock = _context.Blocks
                    .Include(b => b.Transactions)
                    .FirstOrDefault(b => b.BlockNumber == block.BlockNumber);

                if (dbBlock == null)
                {
                    return false; // Block không tồn tại trong database
                }

                // Kiểm tra hash của block
                if (dbBlock.BlockHash != block.BlockHash)
                {
                    return false; // Hash của block đã bị thay đổi
                }

                // Tạo block tạm thời để tính toán lại Merkle Root từ dữ liệu trong database
                var tempBlock = new Block(dbBlock.BlockNumber)
                {
                    CreatedDate = dbBlock.CreatedDate,
                    PreviousBlockHash = dbBlock.PreviousBlockHash
                };

                // Thêm các transaction từ database vào block tạm thời
                foreach (var txn in dbBlock.Transactions)
                {
                    tempBlock.AddTransaction(new Transaction
                    {
                        masinhvien = txn.masinhvien,
                        mamonhoc = txn.mamonhoc,
                        diem = txn.diem,
                        ngayluudiem = txn.ngayluudiem,
                        diemlanthu = txn.diemlanthu
                    });
                }

                // Tính toán lại Merkle Root từ dữ liệu trong database
                string calculatedMerkleRoot = tempBlock.CalculateMerkleRoot();
                
                // So sánh với Merkle Root đã lưu trong database
                if (calculatedMerkleRoot != dbBlock.MerkleRoot)
                {
                    return false; // Dữ liệu giao dịch đã bị sửa đổi
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            // Log lỗi
            Console.WriteLine($"Lỗi khi kiểm tra tính toàn vẹn: {ex.Message}");
            return false;
        }
    }

    public bool EditTransaction(int blockNumber, int transactionId, string masinhvien, string mamonhoc, int diem, DateTime ngayluudiem, int diemlanthu)
    {
        try
        {
            // Lấy block từ database
            var dbBlock = _context.Blocks
                .Include(b => b.Transactions)
                .FirstOrDefault(b => b.BlockNumber == blockNumber);

            if (dbBlock == null)
            {
                return false; // Block không tồn tại
            }

            // Tìm transaction cần sửa
            var transaction = dbBlock.Transactions.FirstOrDefault(t => t.Id == transactionId);
            if (transaction == null)
            {
                return false; // Transaction không tồn tại
            }

            // Tạo block tạm thời để tính toán Merkle Root hiện tại
            var tempBlock = new Block(dbBlock.BlockNumber)
            {
                CreatedDate = dbBlock.CreatedDate,
                PreviousBlockHash = dbBlock.PreviousBlockHash
            };

            // Thêm các transaction hiện tại vào block tạm thời
            foreach (var txn in dbBlock.Transactions)
            {
                tempBlock.AddTransaction(new Transaction
                {
                    masinhvien = txn.masinhvien,
                    mamonhoc = txn.mamonhoc,
                    diem = txn.diem,
                    ngayluudiem = txn.ngayluudiem,
                    diemlanthu = txn.diemlanthu
                });
            }

            // Tính toán Merkle Root hiện tại
            string currentMerkleRoot = tempBlock.CalculateMerkleRoot();

            // Kiểm tra xem Merkle Root hiện tại có khớp với Merkle Root đã lưu không
            if (currentMerkleRoot != dbBlock.MerkleRoot)
            {
                return false; // Dữ liệu đã bị sửa đổi trước đó
            }

            // Nếu Merkle Root khớp, tiến hành sửa đổi
            transaction.masinhvien = masinhvien;
            transaction.mamonhoc = mamonhoc;
            transaction.diem = diem;
            transaction.ngayluudiem = ngayluudiem;
            transaction.diemlanthu = diemlanthu;
            transaction.TransactionHash = new Transaction
            {
                masinhvien = masinhvien,
                mamonhoc = mamonhoc,
                diem = diem,
                ngayluudiem = ngayluudiem,
                diemlanthu = diemlanthu
            }.CalculateTransactionHash();

            // Tính toán lại Merkle Root sau khi sửa đổi
            var updatedTempBlock = new Block(dbBlock.BlockNumber)
            {
                CreatedDate = dbBlock.CreatedDate,
                PreviousBlockHash = dbBlock.PreviousBlockHash
            };

            // Thêm các transaction đã cập nhật vào block tạm thời
            foreach (var txn in dbBlock.Transactions)
            {
                updatedTempBlock.AddTransaction(new Transaction
                {
                    masinhvien = txn.masinhvien,
                    mamonhoc = txn.mamonhoc,
                    diem = txn.diem,
                    ngayluudiem = txn.ngayluudiem,
                    diemlanthu = txn.diemlanthu
                });
            }

            // Tính toán Merkle Root mới
            string newMerkleRoot = updatedTempBlock.CalculateMerkleRoot();

            // Cập nhật Merkle Root mới vào block
            dbBlock.MerkleRoot = newMerkleRoot;

            // Lưu thay đổi vào database
            _context.SaveChanges();

            // Cập nhật lại blockchain trong bộ nhớ
            LoadBlockchainFromDatabase();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi khi sửa transaction: {ex.Message}");
            return false;
        }
    }
}