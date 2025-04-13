using Quanlydiem.Models;
using Quanlydiem.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

/*
HƯỚNG DẪN THAY ĐỔI THUỘC TÍNH:
Trong file này, có 5 thuộc tính chính có thể được thay đổi:
1. tochuccap/tochuccap: Hiện tại là "Mã sinh viên", có thể thay đổi thành thuộc tính khác
2. hoten/hoten: Hiện tại là "Mã môn học", có thể thay đổi thành thuộc tính khác
3. diem/Diem: Hiện tại là "Điểm", có thể thay đổi thành thuộc tính khác
4. ngaycap/ngaycap: Hiện tại là "Ngày lưu điểm", có thể thay đổi thành thuộc tính khác
5. loaitotnghiep/loaitotnghiep: Hiện tại là "Điểm lần thứ", có thể thay đổi thành thuộc tính khác

Các vị trí cần thay đổi:
- Dòng 42-67: Tạo các transaction mẫu trong InitializeBlockchain
- Dòng 83-90: Thêm transaction vào blockModel
- Dòng 318: Tham số của hàm EditTransaction
- Dòng 351-356: Mapping từ transaction sang tempBlock
- Dòng 367-372: Cập nhật transaction
- Dòng 373-379: Tạo transaction mới để tính hash
- Dòng 392-398: Thêm transaction vào updatedTempBlock
*/

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
                    // THUỘC TÍNH 1: Có thể thay đổi tochuccap
                    tochuccap = "Thanglong Uni",
                    // THUỘC TÍNH 2: Có thể thay đổi hoten
                    hoten = "Vũ Nghị",
                    // THUỘC TÍNH 3: Có thể thay đổi diem
                    socancuoc = "123456a",
                    // THUỘC TÍNH 4: Có thể thay đổi ngaycap
                    ngaycap = DateTime.UtcNow,
                    // THUỘC TÍNH 5: Có thể thay đổi loaitotnghiep
                    loaitotnghiep = LoaiTotNghiep.XuatSac
                },
                new Transaction
                {
                    // THUỘC TÍNH 1: Có thể thay đổi tochuccap
                    tochuccap = "Thanglong University",
                    // THUỘC TÍNH 2: Có thể thay đổi hoten
                    hoten = "Vũ Nghị",
                    // THUỘC TÍNH 3: Có thể thay đổi diem
                    socancuoc = "123456aaaaaaaa",
                    // THUỘC TÍNH 4: Có thể thay đổi ngaycap
                    ngaycap = DateTime.UtcNow,
                    // THUỘC TÍNH 5: Có thể thay đổi loaitotnghiep
                    loaitotnghiep = LoaiTotNghiep.Trungbinh
                },
                new Transaction
                {
                    // THUỘC TÍNH 1: Có thể thay đổi tochuccap
                    tochuccap = "Thăng Long",
                    // THUỘC TÍNH 2: Có thể thay đổi hoten
                    hoten = "Hoàng Vũ Nghị",
                    // THUỘC TÍNH 3: Có thể thay đổi diem
                    socancuoc = "a455555",
                    // THUỘC TÍNH 4: Có thể thay đổi ngaycap
                    ngaycap = DateTime.UtcNow,
                    // THUỘC TÍNH 5: Có thể thay đổi loaitotnghiep
                    loaitotnghiep = LoaiTotNghiep.Gioi
                },
                new Transaction
                {
                    // THUỘC TÍNH 1: Có thể thay đổi tochuccap
                    tochuccap = "Thanglong",
                    // THUỘC TÍNH 2: Có thể thay đổi hoten
                    hoten = "Hoàng Vũ Nghị",
                    // THUỘC TÍNH 3: Có thể thay đổi diem
                    socancuoc = "9666aa",
                    // THUỘC TÍNH 4: Có thể thay đổi ngaycap
                    ngaycap = DateTime.UtcNow,
                    // THUỘC TÍNH 5: Có thể thay đổi loaitotnghiep
                    loaitotnghiep = LoaiTotNghiep.Kha
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
                    // THUỘC TÍNH 1: Có thể thay đổi tochuccap
                    tochuccap = transaction.tochuccap,
                    // THUỘC TÍNH 2: Có thể thay đổi hoten
                    hoten = transaction.hoten,
                    // THUỘC TÍNH 3: Có thể thay đổi diem
                    socancuoc = transaction.socancuoc,
                    // THUỘC TÍNH 4: Có thể thay đổi ngaycap
                    ngaycap = transaction.ngaycap,
                    // THUỘC TÍNH 5: Có thể thay đổi loaitotnghiep
                    loaitotnghiep = transaction.loaitotnghiep,
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
                        // THUỘC TÍNH 1: Có thể thay đổi tochuccap
                        tochuccap = transaction.tochuccap,
                        // THUỘC TÍNH 2: Có thể thay đổi hoten
                        hoten = transaction.hoten,
                        // THUỘC TÍNH 3: Có thể thay đổi diem
                        socancuoc = transaction.socancuoc,
                        // THUỘC TÍNH 4: Có thể thay đổi ngaycap
                        ngaycap = transaction.ngaycap,
                        // THUỘC TÍNH 5: Có thể thay đổi loaitotnghiep
                        loaitotnghiep = transaction.loaitotnghiep,
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
                        // THUỘC TÍNH 1: Có thể thay đổi tochuccap
                        tochuccap = transaction.tochuccap,
                        // THUỘC TÍNH 2: Có thể thay đổi hoten
                        hoten = transaction.hoten,
                        // THUỘC TÍNH 3: Có thể thay đổi diem
                        socancuoc = transaction.socancuoc,
                        // THUỘC TÍNH 4: Có thể thay đổi ngaycap
                        ngaycap = transaction.ngaycap,
                        // THUỘC TÍNH 5: Có thể thay đổi loaitotnghiep
                        loaitotnghiep = transaction.loaitotnghiep,
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
                    // THUỘC TÍNH 1: Có thể thay đổi tochuccap
                    tochuccap = transactionModel.tochuccap,
                    // THUỘC TÍNH 2: Có thể thay đổi hoten
                    hoten = transactionModel.hoten,
                    // THUỘC TÍNH 3: Có thể thay đổi diem
                    socancuoc = transactionModel.socancuoc,
                    // THUỘC TÍNH 4: Có thể thay đổi ngaycap
                    ngaycap = transactionModel.ngaycap,
                    // THUỘC TÍNH 5: Có thể thay đổi loaitotnghiep
                    loaitotnghiep = transactionModel.loaitotnghiep
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
                // THUỘC TÍNH 1: Có thể thay đổi tochuccap
                tochuccap = txn.tochuccap,
                // THUỘC TÍNH 2: Có thể thay đổi hoten
                hoten = txn.hoten,
                // THUỘC TÍNH 3: Có thể thay đổi diem
                socancuoc = txn.socancuoc,
                // THUỘC TÍNH 4: Có thể thay đổi ngaycap
                ngaycap = txn.ngaycap,
                // THUỘC TÍNH 5: Có thể thay đổi loaitotnghiep
                loaitotnghiep = txn.loaitotnghiep
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
                        // THUỘC TÍNH 1: Có thể thay đổi tochuccap
                        tochuccap = txn.tochuccap,
                        // THUỘC TÍNH 2: Có thể thay đổi hoten
                        hoten = txn.hoten,
                        // THUỘC TÍNH 3: Có thể thay đổi diem
                        socancuoc = txn.socancuoc,
                        // THUỘC TÍNH 4: Có thể thay đổi ngaycap
                        ngaycap = txn.ngaycap,
                        // THUỘC TÍNH 5: Có thể thay đổi loaitotnghiep
                        loaitotnghiep = txn.loaitotnghiep
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

    // THUỘC TÍNH 1-5: Tham số của hàm EditTransaction có thể thay đổi
    public bool EditTransaction(int blockNumber, int transactionId, string tochuccap, string hoten, string socancuoc, DateTime ngaycap, LoaiTotNghiep loaitotnghiep)
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
                    // THUỘC TÍNH 1: Có thể thay đổi tochuccap
                    tochuccap = txn.tochuccap,
                    // THUỘC TÍNH 2: Có thể thay đổi hoten
                    hoten = txn.hoten,
                    // THUỘC TÍNH 3: Có thể thay đổi diem
                    socancuoc = txn.socancuoc,
                    // THUỘC TÍNH 4: Có thể thay đổi ngaycap
                    ngaycap = txn.ngaycap,
                    // THUỘC TÍNH 5: Có thể thay đổi loaitotnghiep
                    loaitotnghiep = txn.loaitotnghiep
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
            // THUỘC TÍNH 1: Có thể thay đổi tochuccap
            transaction.tochuccap = tochuccap;
            // THUỘC TÍNH 2: Có thể thay đổi hoten
            transaction.hoten = hoten;
            // THUỘC TÍNH 3: Có thể thay đổi diem
            transaction.socancuoc = socancuoc;
            // THUỘC TÍNH 4: Có thể thay đổi ngaycap
            transaction.ngaycap = ngaycap;
            // THUỘC TÍNH 5: Có thể thay đổi loaitotnghiep
            transaction.loaitotnghiep = loaitotnghiep;
            transaction.TransactionHash = new Transaction
            {
                // THUỘC TÍNH 1: Có thể thay đổi tochuccap
                tochuccap = tochuccap,
                // THUỘC TÍNH 2: Có thể thay đổi hoten
                hoten = hoten,
                // THUỘC TÍNH 3: Có thể thay đổi diem
                socancuoc = socancuoc,
                // THUỘC TÍNH 4: Có thể thay đổi ngaycap
                ngaycap = ngaycap,
                // THUỘC TÍNH 5: Có thể thay đổi loaitotnghiep
                loaitotnghiep = loaitotnghiep
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
                    // THUỘC TÍNH 1: Có thể thay đổi tochuccap
                    tochuccap = txn.tochuccap,
                    // THUỘC TÍNH 2: Có thể thay đổi hoten
                    hoten = txn.hoten,
                    // THUỘC TÍNH 3: Có thể thay đổi diem
                    socancuoc = txn.socancuoc,
                    // THUỘC TÍNH 4: Có thể thay đổi ngaycap
                    ngaycap = txn.ngaycap,
                    // THUỘC TÍNH 5: Có thể thay đổi loaitotnghiep
                    loaitotnghiep = txn.loaitotnghiep
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