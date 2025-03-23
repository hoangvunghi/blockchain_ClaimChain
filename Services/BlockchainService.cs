using Quanlydiem.Models;
using Quanlydiem.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

/*
HƯỚNG DẪN THAY ĐỔI THUỘC TÍNH:
Trong file này, có 5 thuộc tính chính có thể được thay đổi:
1. masinhvien/MaSinhVien: Hiện tại là "Mã sinh viên", có thể thay đổi thành thuộc tính khác
2. mamonhoc/MaMonHoc: Hiện tại là "Mã môn học", có thể thay đổi thành thuộc tính khác
3. diem/Diem: Hiện tại là "Điểm", có thể thay đổi thành thuộc tính khác
4. ngayluudiem/NgayLuuDiem: Hiện tại là "Ngày lưu điểm", có thể thay đổi thành thuộc tính khác
5. diemlanthu/DiemLanThu: Hiện tại là "Điểm lần thứ", có thể thay đổi thành thuộc tính khác

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
                    // THUỘC TÍNH 1: Có thể thay đổi masinhvien
                    masinhvien = "A45156",
                    // THUỘC TÍNH 2: Có thể thay đổi mamonhoc
                    mamonhoc = "INT1001",
                    // THUỘC TÍNH 3: Có thể thay đổi diem
                    diem = 8,
                    // THUỘC TÍNH 4: Có thể thay đổi ngayluudiem
                    ngayluudiem = DateTime.UtcNow,
                    // THUỘC TÍNH 5: Có thể thay đổi diemlanthu
                    diemlanthu = 1,
                    // THUỘC TÍNH 6: Có thể thay đổi ImageData
                    ImageData = "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQABAA...",
                    // THUỘC TÍNH 7: Có thể thay đổi HasImage
                    HasImage = true
                },
                new Transaction
                {
                    // THUỘC TÍNH 1: Có thể thay đổi masinhvien
                    masinhvien = "A45157",
                    // THUỘC TÍNH 2: Có thể thay đổi mamonhoc
                    mamonhoc = "INT1001",
                    // THUỘC TÍNH 3: Có thể thay đổi diem
                    diem = 9,
                    // THUỘC TÍNH 4: Có thể thay đổi ngayluudiem
                    ngayluudiem = DateTime.UtcNow,
                    // THUỘC TÍNH 5: Có thể thay đổi diemlanthu
                    diemlanthu = 1,
                    // THUỘC TÍNH 6: Có thể thay đổi ImageData
                    ImageData = "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQABAA...",
                    // THUỘC TÍNH 7: Có thể thay đổi HasImage
                    HasImage = true
                },
                new Transaction
                {
                    // THUỘC TÍNH 1: Có thể thay đổi masinhvien
                    masinhvien = "A45158",
                    // THUỘC TÍNH 2: Có thể thay đổi mamonhoc
                    mamonhoc = "INT1001",
                    // THUỘC TÍNH 3: Có thể thay đổi diem
                    diem = 10,
                    // THUỘC TÍNH 4: Có thể thay đổi ngayluudiem
                    ngayluudiem = DateTime.UtcNow,
                    // THUỘC TÍNH 5: Có thể thay đổi diemlanthu
                    diemlanthu = 1,
                    // THUỘC TÍNH 6: Có thể thay đổi ImageData
                    ImageData = "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQABAA...",
                    // THUỘC TÍNH 7: Có thể thay đổi HasImage
                    HasImage = true
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
                    // THUỘC TÍNH 1: Có thể thay đổi masinhvien
                    masinhvien = transaction.masinhvien,
                    // THUỘC TÍNH 2: Có thể thay đổi mamonhoc
                    mamonhoc = transaction.mamonhoc,
                    // THUỘC TÍNH 3: Có thể thay đổi diem
                    diem = transaction.diem,
                    // THUỘC TÍNH 4: Có thể thay đổi ngayluudiem
                    ngayluudiem = transaction.ngayluudiem,
                    // THUỘC TÍNH 5: Có thể thay đổi diemlanthu
                    diemlanthu = transaction.diemlanthu,
                    // Lưu thông tin hình ảnh
                    ImageData = transaction.ImageData,
                    HasImage = transaction.HasImage,
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
        // Thêm thông báo gỡ lỗi để xác nhận giao dịch được thêm vào
        Console.WriteLine("=== THÊM TRANSACTION MỚI ===");
        Console.WriteLine($"Mã sinh viên: {transaction.masinhvien}");
        Console.WriteLine($"Mã môn học: {transaction.mamonhoc}");
        Console.WriteLine($"Điểm: {transaction.diem}");
        Console.WriteLine($"Ngày lưu điểm: {transaction.ngayluudiem}");
        Console.WriteLine($"Điểm lần thứ: {transaction.diemlanthu}");
        Console.WriteLine($"Có ảnh: {(transaction.HasImage ? "Có" : "Không")}");
        Console.WriteLine($"Độ dài dữ liệu ảnh: {(transaction.ImageData?.Length ?? 0)}");
        Console.WriteLine("=== KẾT THÚC THÊM TRANSACTION ===");
        
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
                        // THUỘC TÍNH 1: Có thể thay đổi masinhvien
                        masinhvien = transaction.masinhvien,
                        // THUỘC TÍNH 2: Có thể thay đổi mamonhoc
                        mamonhoc = transaction.mamonhoc,
                        // THUỘC TÍNH 3: Có thể thay đổi diem
                        diem = transaction.diem,
                        // THUỘC TÍNH 4: Có thể thay đổi ngayluudiem
                        ngayluudiem = transaction.ngayluudiem,
                        // THUỘC TÍNH 5: Có thể thay đổi diemlanthu
                        diemlanthu = transaction.diemlanthu,
                        // Lưu thông tin hình ảnh
                        ImageData = transaction.ImageData,
                        HasImage = transaction.HasImage,
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
                        // THUỘC TÍNH 1: Có thể thay đổi masinhvien
                        masinhvien = transaction.masinhvien,
                        // THUỘC TÍNH 2: Có thể thay đổi mamonhoc
                        mamonhoc = transaction.mamonhoc,
                        // THUỘC TÍNH 3: Có thể thay đổi diem
                        diem = transaction.diem,
                        // THUỘC TÍNH 4: Có thể thay đổi ngayluudiem
                        ngayluudiem = transaction.ngayluudiem,
                        // THUỘC TÍNH 5: Có thể thay đổi diemlanthu
                        diemlanthu = transaction.diemlanthu,
                        // Lưu thông tin hình ảnh
                        ImageData = transaction.ImageData,
                        HasImage = transaction.HasImage,
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
                    // THUỘC TÍNH 1: Có thể thay đổi masinhvien
                    masinhvien = transactionModel.masinhvien,
                    // THUỘC TÍNH 2: Có thể thay đổi mamonhoc
                    mamonhoc = transactionModel.mamonhoc,
                    // THUỘC TÍNH 3: Có thể thay đổi diem
                    diem = transactionModel.diem,
                    // THUỘC TÍNH 4: Có thể thay đổi ngayluudiem
                    ngayluudiem = transactionModel.ngayluudiem,
                    // THUỘC TÍNH 5: Có thể thay đổi diemlanthu
                    diemlanthu = transactionModel.diemlanthu,
                    // Tải thông tin hình ảnh
                    ImageData = transactionModel.ImageData,
                    HasImage = transactionModel.HasImage
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
                // THUỘC TÍNH 1: Có thể thay đổi masinhvien
                masinhvien = txn.masinhvien,
                // THUỘC TÍNH 2: Có thể thay đổi mamonhoc
                mamonhoc = txn.mamonhoc,
                // THUỘC TÍNH 3: Có thể thay đổi diem
                diem = txn.diem,
                // THUỘC TÍNH 4: Có thể thay đổi ngayluudiem
                ngayluudiem = txn.ngayluudiem,
                // THUỘC TÍNH 5: Có thể thay đổi diemlanthu
                diemlanthu = txn.diemlanthu,
                // Tải thông tin hình ảnh
                ImageData = txn.ImageData,
                HasImage = txn.HasImage
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
                        // THUỘC TÍNH 1: Có thể thay đổi masinhvien
                        masinhvien = txn.masinhvien,
                        // THUỘC TÍNH 2: Có thể thay đổi mamonhoc
                        mamonhoc = txn.mamonhoc,
                        // THUỘC TÍNH 3: Có thể thay đổi diem
                        diem = txn.diem,
                        // THUỘC TÍNH 4: Có thể thay đổi ngayluudiem
                        ngayluudiem = txn.ngayluudiem,
                        // THUỘC TÍNH 5: Có thể thay đổi diemlanthu
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

    // THUỘC TÍNH 1-5: Tham số của hàm EditTransaction có thể thay đổi
    public bool EditTransaction(int blockNumber, int transactionId, string masinhvien, string mamonhoc, int diem, DateTime ngayluudiem, int diemlanthu, string imageData = null, bool hasImage = false)
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
                    // THUỘC TÍNH 1: Có thể thay đổi masinhvien
                    masinhvien = txn.masinhvien,
                    // THUỘC TÍNH 2: Có thể thay đổi mamonhoc
                    mamonhoc = txn.mamonhoc,
                    // THUỘC TÍNH 3: Có thể thay đổi diem
                    diem = txn.diem,
                    // THUỘC TÍNH 4: Có thể thay đổi ngayluudiem
                    ngayluudiem = txn.ngayluudiem,
                    // THUỘC TÍNH 5: Có thể thay đổi diemlanthu
                    diemlanthu = txn.diemlanthu,
                    // Thêm thông tin hình ảnh
                    ImageData = txn.ImageData,
                    HasImage = txn.HasImage
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
            // THUỘC TÍNH 1: Có thể thay đổi masinhvien
            transaction.masinhvien = masinhvien;
            // THUỘC TÍNH 2: Có thể thay đổi mamonhoc
            transaction.mamonhoc = mamonhoc;
            // THUỘC TÍNH 3: Có thể thay đổi diem
            transaction.diem = diem;
            // THUỘC TÍNH 4: Có thể thay đổi ngayluudiem
            transaction.ngayluudiem = ngayluudiem;
            // THUỘC TÍNH 5: Có thể thay đổi diemlanthu
            transaction.diemlanthu = diemlanthu;
            // Cập nhật thông tin hình ảnh
            if (hasImage && !string.IsNullOrEmpty(imageData))
            {
                transaction.ImageData = imageData;
                transaction.HasImage = true;
            }
            else if (!hasImage)
            {
                transaction.ImageData = null;
                transaction.HasImage = false;
            }
            
            transaction.TransactionHash = new Transaction
            {
                // THUỘC TÍNH 1: Có thể thay đổi masinhvien
                masinhvien = masinhvien,
                // THUỘC TÍNH 2: Có thể thay đổi mamonhoc
                mamonhoc = mamonhoc,
                // THUỘC TÍNH 3: Có thể thay đổi diem
                diem = diem,
                // THUỘC TÍNH 4: Có thể thay đổi ngayluudiem
                ngayluudiem = ngayluudiem,
                // THUỘC TÍNH 5: Có thể thay đổi diemlanthu
                diemlanthu = diemlanthu,
                // Thêm thông tin hình ảnh vào quá trình tính hash
                ImageData = transaction.ImageData,
                HasImage = transaction.HasImage
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
                    // THUỘC TÍNH 1: Có thể thay đổi masinhvien
                    masinhvien = txn.masinhvien,
                    // THUỘC TÍNH 2: Có thể thay đổi mamonhoc
                    mamonhoc = txn.mamonhoc,
                    // THUỘC TÍNH 3: Có thể thay đổi diem
                    diem = txn.diem,
                    // THUỘC TÍNH 4: Có thể thay đổi ngayluudiem
                    ngayluudiem = txn.ngayluudiem,
                    // THUỘC TÍNH 5: Có thể thay đổi diemlanthu
                    diemlanthu = txn.diemlanthu,
                    // Thêm thông tin hình ảnh
                    ImageData = txn.ImageData,
                    HasImage = txn.HasImage
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