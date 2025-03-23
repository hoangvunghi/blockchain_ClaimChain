using Microsoft.AspNetCore.Mvc;
using Quanlydiem.Services;
using Quanlydiem.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Quanlydiem.Controllers
{
    public class ImageController : Controller
    {
        private readonly ImageService _imageService;
        private readonly BlockchainService _blockchainService;

        public ImageController(ImageService imageService, BlockchainService blockchainService)
        {
            _imageService = imageService;
            _blockchainService = blockchainService;
        }

        // GET: /Image
        public IActionResult Index()
        {
            // Lấy danh sách các transaction có chứa ảnh
            var imageTransactions = new List<Transaction>();
            
            foreach (var block in _blockchainService.Blockchain.Blocks)
            {
                foreach (var transaction in block.Transactions)
                {
                    if (transaction is Transaction tx && tx.HasImage && !string.IsNullOrEmpty(tx.ImageData))
                    {
                        imageTransactions.Add(tx);
                    }
                }
            }
            
            return View(imageTransactions);
        }
        
        // GET: /Image/Upload
        public IActionResult Upload()
        {
            return View();
        }
        
        // POST: /Image/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(string masinhvien, string mamonhoc, IFormFile imageFile)
        {
            if (string.IsNullOrEmpty(masinhvien) || string.IsNullOrEmpty(mamonhoc) || imageFile == null)
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin và chọn ảnh");
                return View();
            }
            
            try
            {
                // Chuyển đổi ảnh sang Base64
                string base64Image = await _imageService.ConvertImageToBase64Async(imageFile);
                
                if (string.IsNullOrEmpty(base64Image))
                {
                    ModelState.AddModelError("", "Không thể đọc dữ liệu ảnh");
                    return View();
                }
                
                // Tạo transaction chứa ảnh và thêm vào blockchain
                var transaction = _imageService.CreateImageTransaction(masinhvien, mamonhoc, base64Image);
                
                TempData["SuccessMessage"] = "Ảnh đã được thêm vào blockchain thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                return View();
            }
        }
        
        // GET: /Image/Details/5
        public IActionResult Details(int blockNumber, int transactionIndex)
        {
            if (blockNumber < 0 || transactionIndex < 0)
                return NotFound();
                
            // Tìm block và transaction
            var block = _blockchainService.Blockchain.Blocks.Find(b => b.BlockNumber == blockNumber);
            
            if (block == null || transactionIndex >= block.Transactions.Count)
                return NotFound();
                
            var transaction = block.Transactions[transactionIndex];
            
            if (!(transaction is Transaction tx) || !tx.HasImage || string.IsNullOrEmpty(tx.ImageData))
                return NotFound();
                
            return View(tx);
        }
    }
} 