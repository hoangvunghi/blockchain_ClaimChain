using Microsoft.AspNetCore.Mvc;
using Quanlydiem.Interfaces;
using Quanlydiem.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Threading.Tasks;

/*
HƯỚNG DẪN THAY ĐỔI THUỘC TÍNH:
Trong file này, có 5 thuộc tính chính có thể được thay đổi:
1. masinhvien/MaSinhVien: Hiện tại là "Mã sinh viên", có thể thay đổi thành thuộc tính khác
2. mamonhoc/MaMonHoc: Hiện tại là "Mã môn học", có thể thay đổi thành thuộc tính khác
3. diem/Diem: Hiện tại là "Điểm", có thể thay đổi thành thuộc tính khác
4. ngayluudiem/NgayLuuDiem: Hiện tại là "Ngày lưu điểm", có thể thay đổi thành thuộc tính khác
5. diemlanthu/DiemLanThu: Hiện tại là "Điểm lần thứ", có thể thay đổi thành thuộc tính khác

Các vị trí cần thay đổi:
- Dòng 76-81: Mapping từ transaction sang EditTransactionViewModel
- Dòng 106-112: Tham số của hàm EditTransaction trong BlockchainService
*/

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
        public async Task<IActionResult> AddTransaction(Transaction model, IFormFile imageFile)
        {
            Console.WriteLine("=== BẮT ĐẦU XỬ LÝ THÊM GIAO DỊCH ===");
            Console.WriteLine($"Model valid: {ModelState.IsValid}");
            Console.WriteLine($"Có file: {(imageFile != null ? "Có" : "Không")}");
            Console.WriteLine($"Kích thước file: {imageFile?.Length ?? 0} bytes");
            
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh nếu có
                if (imageFile != null && imageFile.Length > 0)
                {
                    Console.WriteLine("Đang xử lý file hình ảnh...");
                    // Giới hạn kích thước file (5MB)
                    if (imageFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("", "Kích thước ảnh quá lớn. Vui lòng chọn ảnh dưới 5MB.");
                        Console.WriteLine("Lỗi: File quá lớn");
                        return View("Index", new BlockchainViewModel 
                        { 
                            Blockchain = _blockchainService.Blockchain,
                            PendingTransactions = _blockchainService.GetPendingTransactions(),
                            IsBlockchainValid = _blockchainService.VerifyBlockchainIntegrity()
                        });
                    }
                    
                    // Chuyển đổi ảnh thành Base64
                    using (var memoryStream = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(memoryStream);
                        byte[] imageBytes = memoryStream.ToArray();
                        model.ImageData = Convert.ToBase64String(imageBytes);
                        model.HasImage = true;
                        Console.WriteLine($"Đã chuyển đổi ảnh thành base64 với độ dài: {model.ImageData.Length}");
                    }
                }
                else
                {
                    Console.WriteLine("Không có ảnh được tải lên");
                    model.HasImage = false;
                    model.ImageData = null;
                }
                
                Console.WriteLine("Gọi BlockchainService.AddTransaction...");
                _blockchainService.AddTransaction(model);
                Console.WriteLine("Đã thêm giao dịch thành công");
                return RedirectToAction("Index");
            }
            
            Console.WriteLine("ModelState không hợp lệ:");
            foreach (var state in ModelState)
            {
                Console.WriteLine($"- {state.Key}: {string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage))}");
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
                DiemLanThu = transaction.diemlanthu,
                ImageData = transaction.ImageData,
                HasImage = transaction.HasImage
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditTransaction(EditTransactionViewModel model, IFormFile imageFile, bool? removeImage)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh mới nếu có
                string imageData = model.ImageData;
                bool hasImage = model.HasImage;
                
                // Nếu người dùng muốn xóa ảnh
                if (removeImage == true)
                {
                    imageData = null;
                    hasImage = false;
                }
                // Nếu người dùng tải lên ảnh mới
                else if (imageFile != null && imageFile.Length > 0)
                {
                    // Giới hạn kích thước file (5MB)
                    if (imageFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("", "Kích thước ảnh quá lớn. Vui lòng chọn ảnh dưới 5MB.");
                        return View(model);
                    }
                    
                    // Chuyển đổi ảnh thành Base64
                    using (var memoryStream = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(memoryStream);
                        byte[] imageBytes = memoryStream.ToArray();
                        imageData = Convert.ToBase64String(imageBytes);
                        hasImage = true;
                    }
                }
                
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
                    model.DiemLanThu,
                    imageData,
                    hasImage
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