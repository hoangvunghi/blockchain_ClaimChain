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

        public BlockchainIntegrityController(BlockChain blockchain)
        {
            _blockchain = blockchain;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Kiểm tra tính toàn vẹn của blockchain
            var integrityResults = _blockchain.VerifyBlockchainIntegrity();
            
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
            
            // Kiểm tra tính toàn vẹn của block
            var integrityService = new BlockchainIntegrityService(_blockchain);
            var blockIntegrity = integrityService.VerifyBlockIntegrity(block);
            
            // Thêm thông tin giao dịch để hiển thị
            ViewBag.Transactions = block.Transactions;
            
            return View(blockIntegrity);
        }
        
        [HttpGet]
        public JsonResult GetBlockchainIntegrityStatus()
        {
            // API endpoint để kiểm tra tình trạng của blockchain
            var results = _blockchain.VerifyBlockchainIntegrity();
            
            return Json(new
            {
                IsValid = results.Count == 0,
                ModifiedBlocks = results,
                TotalBlocks = _blockchain.Blocks.Count
            });
        }
    }
} 