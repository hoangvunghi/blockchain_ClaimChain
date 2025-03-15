using Microsoft.AspNetCore.Mvc;
using Quanlydiem.Interfaces;
using Quanlydiem.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Quanlydiem.Controllers
{
    public class HomeController : Controller
    {
        private readonly BlockchainService _blockchainService;
        private readonly BlockChainContext _context;

        public HomeController(BlockchainService blockchainService, BlockChainContext context)
        {
            _blockchainService = blockchainService;
            _context = context;
        }

        public IActionResult Index()
        {
            var model = new BlockchainViewModel
            {
                Blockchain = _blockchainService.Blockchain,
                PendingTransactions = _blockchainService.GetPendingTransactions(),
                IsBlockchainValid = _blockchainService.VerifyBlockchainIntegrity()
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult AddTransaction(Transaction model)
        {
            if (ModelState.IsValid)
            {
                _blockchainService.AddTransaction(model);
                return RedirectToAction("Index");
            }
            return View("Index", new BlockchainViewModel 
            { 
                Blockchain = _blockchainService.Blockchain,
                PendingTransactions = _blockchainService.GetPendingTransactions(),
                IsBlockchainValid = _blockchainService.VerifyBlockchainIntegrity()
            });
        }

        [HttpGet]
        public IActionResult VerifyBlockchain()
        {
            bool isValid = _blockchainService.VerifyBlockchainIntegrity();
            return Json(new { isValid });
        }

        [HttpGet]
        public IActionResult EditTransaction(int blockNumber, int transactionId)
        {
            // Lấy block từ database
            var block = _blockchainService.Blockchain.Blocks.FirstOrDefault(b => b.BlockNumber == blockNumber);
            if (block == null)
            {
                return NotFound();
            }

            // Lấy transaction từ database
            var dbBlock = _context.Blocks
                .Include(b => b.Transactions)
                .FirstOrDefault(b => b.BlockNumber == blockNumber);
            
            if (dbBlock == null)
            {
                return NotFound();
            }

            var transaction = dbBlock.Transactions.FirstOrDefault(t => t.Id == transactionId);
            if (transaction == null)
            {
                return NotFound();
            }

            var model = new EditTransactionViewModel
            {
                BlockNumber = blockNumber,
                TransactionId = transactionId,
                MaSinhVien = transaction.masinhvien,
                MaMonHoc = transaction.mamonhoc,
                Diem = transaction.diem,
                NgayLuuDiem = transaction.ngayluudiem,
                DiemLanThu = transaction.diemlanthu
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult EditTransaction(EditTransactionViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Lấy block từ database để lấy Merkle Root cũ
                var dbBlock = _context.Blocks
                    .Include(b => b.Transactions)
                    .FirstOrDefault(b => b.BlockNumber == model.BlockNumber);
                
                if (dbBlock == null)
                {
                    return NotFound();
                }
                
                string oldMerkleRoot = dbBlock.MerkleRoot;
                
                // Thực hiện sửa transaction
                bool success = _blockchainService.EditTransaction(
                    model.BlockNumber,
                    model.TransactionId,
                    model.MaSinhVien,
                    model.MaMonHoc,
                    model.Diem,
                    model.NgayLuuDiem,
                    model.DiemLanThu
                );

                if (success)
                {
                    // Lấy block sau khi đã cập nhật để lấy Merkle Root mới
                    var updatedBlock = _context.Blocks
                        .FirstOrDefault(b => b.BlockNumber == model.BlockNumber);
                    
                    string newMerkleRoot = updatedBlock.MerkleRoot;
                    
                    // Kiểm tra xem Merkle Root có thay đổi không
                    if (oldMerkleRoot != newMerkleRoot)
                    {
                        TempData["SuccessMessage"] = $"Transaction đã được cập nhật thành công. Merkle Root đã thay đổi từ {oldMerkleRoot.Substring(0, 10)}... thành {newMerkleRoot.Substring(0, 10)}...";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Transaction đã được cập nhật thành công.";
                    }
                    
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Không thể cập nhật transaction. Dữ liệu blockchain đã bị sửa đổi trái phép!");
                }
            }

            return View(model);
        }
    }
}