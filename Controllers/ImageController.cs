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
                    if (transaction is Transaction tx && tx.HasImage && 
                       (!string.IsNullOrEmpty(tx.ImageData) || !string.IsNullOrEmpty(tx.ImageHash)))
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
                // Sử dụng hash thay vì base64
                var transaction = await _imageService.CreateImageHashTransactionAsync(masinhvien, mamonhoc, imageFile);
                
                TempData["SuccessMessage"] = "Hash của ảnh đã được thêm vào blockchain thành công!";
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
            
            if (!(transaction is Transaction tx) || !tx.HasImage || 
               (string.IsNullOrEmpty(tx.ImageData) && string.IsNullOrEmpty(tx.ImageHash)))
                return NotFound();
                
            return View(tx);
        }
        
        // GET: /Image/VerifyImage
        public IActionResult VerifyImage()
        {
            return View();
        }
        
        // POST: /Image/VerifyImage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn file ảnh để xác minh");
                return View();
            }
            
            try
            {
                // Tính toán hash SHA256 của ảnh
                string imageHashToVerify = await _imageService.CreateImageSHA256HashAsync(imageFile);
                
                if (string.IsNullOrEmpty(imageHashToVerify))
                {
                    ModelState.AddModelError("", "Không thể tính toán hash của ảnh");
                    return View();
                }
                
                // Tìm kiếm hash trong blockchain
                var matchingTransactions = new List<Transaction>();
                foreach (var block in _blockchainService.Blockchain.Blocks)
                {
                    foreach (var transaction in block.Transactions)
                    {
                        if (transaction is Transaction tx && tx.HasImage && 
                           tx.ImageHash == imageHashToVerify)
                        {
                            matchingTransactions.Add(tx);
                        }
                    }
                }
                
                ViewBag.ImageHash = imageHashToVerify;
                ViewBag.MatchingCount = matchingTransactions.Count;
                
                return View("VerifyResults", matchingTransactions);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                return View();
            }
        }
    }
} 