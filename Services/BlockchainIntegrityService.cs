using Quanlydiem.Interfaces;
using Quanlydiem.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;

namespace Quanlydiem.Services
{
    public class BlockchainIntegrityService
    {
        private readonly BlockChain _blockchain;
        private readonly BlockChainContext _dbContext;

        public BlockchainIntegrityService(BlockChain blockchain)
        {
            _blockchain = blockchain;
            // Khởi tạo đối tượng DbContext với cấu hình đơn giản
            var options = new DbContextOptionsBuilder<BlockChainContext>()
                .UseSqlite("Data Source=blockchain.db")
                .Options;
            _dbContext = new BlockChainContext(options);
        }

        // Constructor có tham số DbContext để injection
        public BlockchainIntegrityService(BlockChain blockchain, BlockChainContext dbContext)
        {
            _blockchain = blockchain;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Kiểm tra tính toàn vẹn của toàn bộ blockchain
        /// </summary>
        /// <returns>Danh sách các block bị sửa đổi</returns>
        public List<BlockIntegrityResult> VerifyBlockchainIntegrity()
        {
            var results = new List<BlockIntegrityResult>();
            
            foreach (var block in _blockchain.Blocks)
            {
                var integrityResult = VerifyBlockIntegrity(block);
                if (!integrityResult.IsValid)
                {
                    results.Add(integrityResult);
                }
            }
            
            return results;
        }

        /// <summary>
        /// Kiểm tra tính toàn vẹn của một block
        /// </summary>
        /// <param name="block">Block cần kiểm tra</param>
        /// <returns>Kết quả kiểm tra</returns>
        public BlockIntegrityResult VerifyBlockIntegrity(IBlock block)
        {
            // Tính toán lại Merkle root từ các giao dịch hiện tại
            var recalculatedMerkleRoot = block.CalculateMerkleRoot();
            // So sánh với Merkle root đã lưu trong block
            bool isValid = recalculatedMerkleRoot == block.MerkleRoot;
            
            var result = new BlockIntegrityResult
            {
                BlockNumber = block.BlockNumber,
                StoredMerkleRoot = block.MerkleRoot, // Merkle root gốc đã lưu
                CalculatedMerkleRoot = recalculatedMerkleRoot, // Merkle root tính lại
                IsValid = isValid,
                CanDetectSpecificTransaction = true, // Giờ đây chúng ta có thể xác định chính xác
                ModifiedTransactions = new List<int>() // Sẽ được cập nhật nếu có giao dịch bị sửa đổi
            };

            // Nếu Merkle root không khớp, tìm các giao dịch cụ thể đã bị sửa đổi
            if (!isValid)
            {
                result.ModifiedTransactions = FindModifiedTransactions(block);
                result.TransactionHashDetails = GetTransactionHashDetails(block);
            }
            
            return result;
        }

        /// <summary>
        /// Tìm các giao dịch bị sửa đổi trong một block
        /// </summary>
        /// <param name="block">Block cần kiểm tra</param>
        /// <returns>Danh sách index của các giao dịch bị sửa đổi</returns>
        private List<int> FindModifiedTransactions(IBlock block)
        {
            var modifiedTransactions = new List<int>();
            
            try
            {
                // Lấy block model từ database
                var dbBlock = _dbContext.Blocks
                    .Include(b => b.Transactions)
                    .FirstOrDefault(b => b.BlockNumber == block.BlockNumber);
                
                if (dbBlock == null)
                {
                    Console.WriteLine($"Không tìm thấy Block #{block.BlockNumber} trong cơ sở dữ liệu!");
                    return modifiedTransactions;
                }
                
                Console.WriteLine($"Đang kiểm tra {block.Transactions.Count} transactions trong Block #{block.BlockNumber}");
                
                // In danh sách ID của các giao dịch trong database để debug
                var dbTransactionIds = dbBlock.Transactions.Select(t => t.Id).ToList();
                Console.WriteLine($"ID của các giao dịch trong DB: {string.Join(", ", dbTransactionIds)}");
                
                for (int i = 0; i < block.Transactions.Count; i++)
                {
                    var transaction = block.Transactions[i];
                    
                    // Tính toán hash hiện tại dựa trên dữ liệu hiện tại
                    string currentHash = transaction.CalculateTransactionHash();
                    
                    // Thử các cách khác nhau để tìm transaction trong database
                    TransactionModel dbTransaction = null;
                    
                    // Cách 1: Tìm theo index (vị trí tương đối trong block)
                    if (i < dbBlock.Transactions.Count)
                    {
                        dbTransaction = dbBlock.Transactions.OrderBy(t => t.Id).Skip(i).FirstOrDefault();
                    }
                    
                    if (dbTransaction == null)
                    {
                        Console.WriteLine($"Transaction #{i} không tìm thấy trong cơ sở dữ liệu theo index!");
                        
                        // Cách 2: Tìm theo nội dung (matching data)
                        dbTransaction = dbBlock.Transactions.FirstOrDefault(t => 
                            t.tochuccap == transaction.tochuccap &&
                            t.hoten == transaction.hoten &&
                            t.socancuoc == transaction.socancuoc);
                        
                        if (dbTransaction != null)
                        {
                            Console.WriteLine($"Transaction #{i} tìm thấy trong cơ sở dữ liệu theo nội dung (ID: {dbTransaction.Id})");
                        }
                    }
                    
                    if (dbTransaction != null)
                    {
                        // Lấy hash ban đầu từ cơ sở dữ liệu
                        string originalHash = dbTransaction.TransactionHash;
                        
                        // In thông tin debug
                        Console.WriteLine($"Transaction #{i} trong Block #{block.BlockNumber}:");
                        Console.WriteLine($"  - Hash gốc: {originalHash}");
                        Console.WriteLine($"  - Hash hiện tại: {currentHash}");
                        Console.WriteLine($"  - Dữ liệu (memory): {transaction.tochuccap}, {transaction.hoten}, {transaction.socancuoc}");
                        Console.WriteLine($"  - Dữ liệu (DB): {dbTransaction.tochuccap}, {dbTransaction.hoten}, {dbTransaction.socancuoc}");
                        
                        // So sánh hash hiện tại với hash gốc
                        if (!string.IsNullOrEmpty(originalHash) && currentHash != originalHash)
                        {
                            // Nếu khác nhau, transaction đã bị sửa đổi
                            modifiedTransactions.Add(i);
                            
                            // In thêm thông tin chi tiết
                            Console.WriteLine($"  - Đã bị sửa đổi!");
                        }
                        else
                        {
                            Console.WriteLine($"  - Nguyên vẹn");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Transaction #{i} không tìm thấy trong cơ sở dữ liệu sau khi thử nhiều cách!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tìm transactions bị sửa đổi: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            
            return modifiedTransactions;
        }

        /// <summary>
        /// Lấy thông tin chi tiết về hash của các giao dịch
        /// </summary>
        /// <param name="block">Block cần kiểm tra</param>
        /// <returns>Danh sách chi tiết về hash của các giao dịch</returns>
        private List<TransactionHashDetail> GetTransactionHashDetails(IBlock block)
        {
            var hashDetails = new List<TransactionHashDetail>();
            
            Console.WriteLine($"Kiểm tra chi tiết {block.Transactions.Count} transactions trong Block #{block.BlockNumber}");
            
            try
            {
                // Lấy block model từ database
                var dbBlock = _dbContext.Blocks
                    .Include(b => b.Transactions)
                    .FirstOrDefault(b => b.BlockNumber == block.BlockNumber);
                
                if (dbBlock == null)
                {
                    Console.WriteLine($"Không tìm thấy Block #{block.BlockNumber} trong cơ sở dữ liệu!");
                    return hashDetails;
                }
                
                Console.WriteLine($"Đã tìm thấy Block #{block.BlockNumber} trong cơ sở dữ liệu với {dbBlock.Transactions.Count} transactions");
                
                // In danh sách ID của các giao dịch trong database để debug
                var dbTransactionIds = dbBlock.Transactions.Select(t => t.Id).ToList();
                Console.WriteLine($"ID của các giao dịch trong DB: {string.Join(", ", dbTransactionIds)}");
                
                for (int i = 0; i < block.Transactions.Count; i++)
                {
                    var transaction = block.Transactions[i];
                    
                    // Tính toán hash hiện tại dựa trên dữ liệu hiện tại
                    string currentHash = transaction.CalculateTransactionHash();
                    string originalHash = null;
                    bool isModified = false;
                    
                    // Thử các cách khác nhau để tìm transaction trong database
                    TransactionModel dbTransaction = null;
                    
                    // Cách 1: Tìm theo index (vị trí tương đối trong block)
                    if (i < dbBlock.Transactions.Count)
                    {
                        dbTransaction = dbBlock.Transactions.OrderBy(t => t.Id).Skip(i).FirstOrDefault();
                    }
                    
                    if (dbTransaction == null)
                    {
                        Console.WriteLine($"Transaction #{i} không tìm thấy trong cơ sở dữ liệu theo index!");
                        
                        // Cách 2: Tìm theo nội dung (matching data)
                        dbTransaction = dbBlock.Transactions.FirstOrDefault(t => 
                            t.tochuccap == transaction.tochuccap &&
                            t.hoten == transaction.hoten &&
                            t.socancuoc == transaction.socancuoc);
                        
                        if (dbTransaction != null)
                        {
                            Console.WriteLine($"Transaction #{i} tìm thấy trong cơ sở dữ liệu theo nội dung (ID: {dbTransaction.Id})");
                        }
                    }
                    
                    if (dbTransaction != null)
                    {
                        originalHash = dbTransaction.TransactionHash;
                        
                        Console.WriteLine($"Transaction #{i} trong Block #{block.BlockNumber}:");
                        Console.WriteLine($"  - Hash gốc lưu trong DB: {originalHash}");
                        Console.WriteLine($"  - Hash tính toán hiện tại: {currentHash}");
                        Console.WriteLine($"  - Dữ liệu (memory): {transaction.tochuccap}, {transaction.hoten}, {transaction.socancuoc}");
                        Console.WriteLine($"  - Dữ liệu (DB): {dbTransaction.tochuccap}, {dbTransaction.hoten}, {dbTransaction.socancuoc}");
                        
                        // So sánh hash
                        isModified = !string.IsNullOrEmpty(originalHash) && currentHash != originalHash;
                        Console.WriteLine($"  - Đã bị sửa đổi: {isModified}");
                    }
                    else
                    {
                        // Không tìm thấy transaction trong database
                        originalHash = "Không tìm thấy trong DB";
                        isModified = false;
                        Console.WriteLine($"Transaction #{i} không tìm thấy trong cơ sở dữ liệu sau khi thử nhiều cách!");
                    }
                    
                    hashDetails.Add(new TransactionHashDetail
                    {
                        TransactionIndex = i,
                        HoTen = transaction.hoten,
                        SoCanCuoc = transaction.socancuoc,
                        OriginalHash = originalHash,
                        CurrentHash = currentHash,
                        IsModified = isModified
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy thông tin hash chi tiết: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            
            return hashDetails;
        }

        /// <summary>
        /// Kiểm tra tất cả các transaction đã bị sửa đổi trong toàn bộ blockchain
        /// </summary>
        /// <returns>Danh sách các transaction bị sửa đổi kèm thông tin block chứa chúng</returns>
        public List<ModifiedTransactionInfo> FindAllModifiedTransactions()
        {
            Console.WriteLine("=========== BẮT ĐẦU KIỂM TRA TOÀN BỘ TRANSACTIONS ===========");
            var result = new List<ModifiedTransactionInfo>();
            
            try
            {
                Console.WriteLine($"Tổng số blocks trong blockchain: {_blockchain.Blocks.Count}");
                
                foreach (var block in _blockchain.Blocks)
                {
                    Console.WriteLine($"Đang kiểm tra Block #{block.BlockNumber} với {block.Transactions.Count} transactions");
                    
                    // Lấy block từ database
                    var dbBlock = _dbContext.Blocks
                        .Include(b => b.Transactions)
                        .FirstOrDefault(b => b.BlockNumber == block.BlockNumber);
                        
                    if (dbBlock == null)
                    {
                        Console.WriteLine($"  Không tìm thấy Block #{block.BlockNumber} trong database!");
                        continue;
                    }
                    
                    Console.WriteLine($"  Đã tìm thấy Block #{block.BlockNumber} trong database với {dbBlock.Transactions.Count} transactions");
                    
                    // In danh sách ID của các giao dịch trong database để debug
                    var dbTransactionIds = dbBlock.Transactions.Select(t => t.Id).ToList();
                    Console.WriteLine($"  ID của các giao dịch trong DB: {string.Join(", ", dbTransactionIds)}");
                    
                    // Kiểm tra từng transaction trong block
                    for (int i = 0; i < block.Transactions.Count; i++)
                    {
                        var transaction = block.Transactions[i];
                        Console.WriteLine($"  Kiểm tra Transaction #{i} trong Block #{block.BlockNumber}");
                        
                        // Tính toán hash hiện tại dựa trên dữ liệu hiện tại
                        string currentHash = transaction.CalculateTransactionHash();
                        Console.WriteLine($"    Hash hiện tại: {currentHash}");
                        
                        // Thử các cách khác nhau để tìm transaction trong database
                        TransactionModel dbTransaction = null;
                        
                        // Cách 1: Tìm theo index (vị trí tương đối trong block)
                        if (i < dbBlock.Transactions.Count)
                        {
                            dbTransaction = dbBlock.Transactions.OrderBy(t => t.Id).Skip(i).FirstOrDefault();
                        }
                        
                        if (dbTransaction == null)
                        {
                            Console.WriteLine($"    Transaction #{i} không tìm thấy trong cơ sở dữ liệu theo index!");
                            
                            // Cách 2: Tìm theo nội dung (matching data)
                            dbTransaction = dbBlock.Transactions.FirstOrDefault(t => 
                                t.tochuccap == transaction.tochuccap &&
                                t.hoten == transaction.hoten &&
                                t.socancuoc == transaction.socancuoc);
                            
                            if (dbTransaction != null)
                            {
                                Console.WriteLine($"    Transaction #{i} tìm thấy trong cơ sở dữ liệu theo nội dung (ID: {dbTransaction.Id})");
                            }
                        }
                        
                        if (dbTransaction == null)
                        {
                            Console.WriteLine($"    Không tìm thấy Transaction #{i} trong database!");
                            continue;
                        }
                        
                        // Lấy hash ban đầu từ cơ sở dữ liệu
                        string originalHash = dbTransaction.TransactionHash;
                        Console.WriteLine($"    Hash gốc: {originalHash}");
                        Console.WriteLine($"    Dữ liệu (memory): {transaction.tochuccap}, {transaction.hoten}, {transaction.socancuoc}");
                        Console.WriteLine($"    Dữ liệu (DB): {dbTransaction.tochuccap}, {dbTransaction.hoten}, {dbTransaction.socancuoc}");
                        
                        // So sánh hash hiện tại với hash gốc
                        bool isModified = !string.IsNullOrEmpty(originalHash) && currentHash != originalHash;
                        Console.WriteLine($"    Transaction bị sửa đổi: {isModified}");
                        
                        if (isModified)
                        {
                            // Nếu khác nhau, transaction đã bị sửa đổi
                            var modifiedInfo = new ModifiedTransactionInfo
                            {
                                BlockNumber = block.BlockNumber,
                                TransactionIndex = i,
                                TransactionId = dbTransaction.Id,
                                OriginalHash = originalHash,
                                CurrentHash = currentHash,
                                ToChucCap = transaction.tochuccap,
                                HoTen = transaction.hoten,
                                SoCanCuoc = transaction.socancuoc,
                                NgayCap = transaction.ngaycap,
                                LoaiTotNghiep = transaction.loaitotnghiep
                            };
                            
                            result.Add(modifiedInfo);
                            Console.WriteLine($"    >>> Transaction #{i} (ID: {dbTransaction.Id}) trong Block #{block.BlockNumber} đã bị sửa đổi!");
                            Console.WriteLine($"    >>> Chi tiết: {transaction.tochuccap}, {transaction.hoten}, {transaction.socancuoc}");
                        }
                    }
                }
                
                Console.WriteLine($"Kết quả: Tìm thấy {result.Count} transactions đã bị sửa đổi");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LỖI khi kiểm tra các transactions: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            
            Console.WriteLine("=========== KẾT THÚC KIỂM TRA TOÀN BỘ TRANSACTIONS ===========");
            return result;
        }
    }

    public class BlockIntegrityResult
    {
        public int BlockNumber { get; set; }
        public string StoredMerkleRoot { get; set; }
        public string CalculatedMerkleRoot { get; set; }
        public bool IsValid { get; set; }
        /// <summary>
        /// Cho biết có thể xác định chính xác giao dịch nào bị sửa đổi hay không
        /// </summary>
        public bool CanDetectSpecificTransaction { get; set; }
        public List<int> ModifiedTransactions { get; set; } = new List<int>();
        /// <summary>
        /// Thông tin chi tiết về hash của các giao dịch
        /// </summary>
        public List<TransactionHashDetail> TransactionHashDetails { get; set; } = new List<TransactionHashDetail>();
    }

    public class TransactionHashDetail
    {
        public int TransactionIndex { get; set; }
        public string HoTen { get; set; }
        public string SoCanCuoc { get; set; }
        public string OriginalHash { get; set; }
        public string CurrentHash { get; set; }
        public bool IsModified { get; set; }
    }

    /// <summary>
    /// Lớp chứa thông tin chi tiết về transaction bị sửa đổi
    /// </summary>
    public class ModifiedTransactionInfo
    {
        public int BlockNumber { get; set; }
        public int TransactionIndex { get; set; }
        public int TransactionId { get; set; }
        public string OriginalHash { get; set; }
        public string CurrentHash { get; set; }
        public string ToChucCap { get; set; }
        public string HoTen { get; set; }
        public string SoCanCuoc { get; set; }
        public DateTime NgayCap { get; set; }
        public LoaiTotNghiep LoaiTotNghiep { get; set; }
    }
}
