using Microsoft.AspNetCore.Mvc;
using Quanlydiem.Models;
using Quanlydiem.Services;
using System.Collections.Generic;
using System.Linq;

namespace Quanlydiem.Controllers
{
    public class BlockchainIntegrityController : Controller
    {
        private readonly BlockChain _blockchain;
        private readonly BlockChainContext _dbContext;
        private readonly BlockchainIntegrityService _integrityService;

        public BlockchainIntegrityController(BlockChain blockchain, BlockChainContext dbContext, BlockchainIntegrityService integrityService)
        {
            _blockchain = blockchain;
            _dbContext = dbContext;
            _integrityService = integrityService;
            
            // Log để kiểm tra
            Console.WriteLine($"BlockchainIntegrityController: Khởi tạo với BlockchainIntegrityService, blockchain có {blockchain?.Blocks?.Count ?? 0} blocks");
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Kiểm tra tính toàn vẹn của blockchain
            var integrityResults = _integrityService.VerifyBlockchainIntegrity();
            
            // Thêm thông tin tổng quan cho view
            ViewBag.TotalBlocks = _blockchain.Blocks.Count;
            ViewBag.ModifiedBlocksCount = integrityResults.Count;
            ViewBag.IntegrityPercentage = _blockchain.Blocks.Count > 0 
                ? (1 - ((double)integrityResults.Count / _blockchain.Blocks.Count)) * 100 
                : 100;
            
            return View(integrityResults);
        }
        
        [HttpGet]
        public IActionResult BlockDetails(int blockNumber)
        {
            // Tìm block với số thứ tự được chỉ định
            var block = _blockchain.Blocks.FirstOrDefault(b => b.BlockNumber == blockNumber);
            
            if (block == null)
            {
                return NotFound();
            }
            
            Console.WriteLine($"Kiểm tra chi tiết Block #{blockNumber}");
            
            // Kiểm tra tính toàn vẹn của block
            var blockIntegrity = _integrityService.VerifyBlockIntegrity(block);
            
            // Thêm thông tin giao dịch để hiển thị
            ViewBag.Transactions = block.Transactions;
            
            // Thêm thông tin chi tiết về hash của các giao dịch
            ViewBag.TransactionHashDetails = blockIntegrity.TransactionHashDetails;
            ViewBag.HasModifiedTransactions = blockIntegrity.ModifiedTransactions.Any();
            ViewBag.ModifiedTransactionsCount = blockIntegrity.ModifiedTransactions.Count;
            
            // Kiểm tra từng transaction để phát hiện sửa đổi
            Console.WriteLine($"Kiểm tra hash của {block.Transactions.Count} transactions trong Block #{blockNumber}");
            
            var modifiedTransactions = new List<int>();
            foreach (var detail in blockIntegrity.TransactionHashDetails)
            {
                Console.WriteLine($"Transaction #{detail.TransactionIndex}:");
                Console.WriteLine($"  - Original Hash: {detail.OriginalHash}");
                Console.WriteLine($"  - Current Hash: {detail.CurrentHash}");
                Console.WriteLine($"  - Modified: {detail.IsModified}");
                
                if (detail.IsModified)
                {
                    modifiedTransactions.Add(detail.TransactionIndex);
                }
            }
            
            ViewBag.ModifiedTransactions = modifiedTransactions;
            
            // Kiểm tra nếu có sự khác biệt giữa Merkle root và trạng thái transactions
            bool merkleRootChanged = blockIntegrity.CalculatedMerkleRoot != blockIntegrity.StoredMerkleRoot;
            bool transactionsModified = modifiedTransactions.Any();
            
            if (merkleRootChanged && !transactionsModified)
            {
                ViewBag.IntegrityWarning = "Mặc dù Merkle root đã thay đổi, nhưng tất cả các giao dịch đều được xác nhận là nguyên vẹn. Có thể có lỗi trong quá trình tính toán Merkle root.";
            }
            else if (!merkleRootChanged && transactionsModified)
            {
                ViewBag.IntegrityWarning = "Phát hiện các giao dịch bị sửa đổi, nhưng Merkle root vẫn không thay đổi. Điều này không nên xảy ra và có thể là dấu hiệu của sự can thiệp.";
            }
            
            return View(blockIntegrity);
        }
        
        [HttpGet]
        public JsonResult GetBlockchainIntegrityStatus()
        {
            // API endpoint để kiểm tra tình trạng của blockchain
            var results = _integrityService.VerifyBlockchainIntegrity();
            
            // Tạo danh sách các block bị sửa đổi với thông tin chi tiết về các giao dịch bị thay đổi
            var modifiedBlocksWithDetails = results.Select(r => new
            {
                BlockNumber = r.BlockNumber,
                IsValid = r.IsValid,
                StoredMerkleRoot = r.StoredMerkleRoot,
                CalculatedMerkleRoot = r.CalculatedMerkleRoot,
                ModifiedTransactionsCount = r.ModifiedTransactions.Count,
                ModifiedTransactions = r.ModifiedTransactions,
                TransactionHashDetails = r.TransactionHashDetails
                    .Where(t => t.IsModified)
                    .Select(t => new
                    {
                        TransactionIndex = t.TransactionIndex,
                        HoTen = t.HoTen,
                        SoCanCuoc = t.SoCanCuoc,
                        OriginalHash = t.OriginalHash,
                        CurrentHash = t.CurrentHash
                    })
            });
            
            return Json(new
            {
                IsValid = results.Count == 0,
                ModifiedBlocks = modifiedBlocksWithDetails,
                TotalBlocks = _blockchain.Blocks.Count
            });
        }
    }
}