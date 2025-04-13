using Microsoft.AspNetCore.Mvc;
using Quanlydiem.Interfaces;
using Quanlydiem.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Quanlydiem.Services;

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
        private readonly BlockchainIntegrityService _integrityService;

        public HomeController(BlockchainService blockchainService, BlockChainContext context, BlockchainIntegrityService integrityService)
        {
            _blockchainService = blockchainService;
            _context = context;
            _integrityService = integrityService;
            
            // Log để kiểm tra
            Console.WriteLine($"HomeController: Khởi tạo với BlockchainIntegrityService, blockchain có {blockchainService.Blockchain?.Blocks?.Count ?? 0} blocks");
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

        // RSA Encryption Services
        private readonly string PublicKeyFile = "wwwroot/publicKey.xml";
        private readonly string PrivateKeyFile = "wwwroot/privateKey.xml";

        [HttpGet]
        public IActionResult GetModifiedTransactions()
        {
            try
            {
                Console.WriteLine("HomeController: Bắt đầu gọi phương thức FindAllModifiedTransactions");
                var modifiedTransactions = _integrityService.FindAllModifiedTransactions();
                Console.WriteLine($"HomeController: Đã tìm thấy {modifiedTransactions.Count} transactions bị sửa đổi");
                
                // Thêm thông báo nếu có transactions bị sửa đổi
                if (modifiedTransactions.Count > 0)
                {
                    TempData["ErrorMessage"] = $"Phát hiện {modifiedTransactions.Count} transactions đã bị sửa đổi!";
                }
                else
                {
                    TempData["SuccessMessage"] = "Không phát hiện transaction nào bị sửa đổi. Blockchain vẫn nguyên vẹn!";
                }
                
                return View("ModifiedTransactions", modifiedTransactions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi kiểm tra transactions: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi kiểm tra: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult GenerateKeys()
        {
            using (var rsa = new System.Security.Cryptography.RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                string publicKey = rsa.ToXmlString(false);
                string privateKey = rsa.ToXmlString(true);
                System.IO.File.WriteAllText(PublicKeyFile, publicKey);
                System.IO.File.WriteAllText(PrivateKeyFile, privateKey);
            }
            TempData["SuccessMessage"] = "Bộ khóa RSA đã được tạo thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Encrypt(string text)
        {
            if (!System.IO.File.Exists(PublicKeyFile))
                return Json(new { success = false, message = "Không tìm thấy khóa công khai!" });

            using (var rsa = new System.Security.Cryptography.RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                rsa.FromXmlString(System.IO.File.ReadAllText(PublicKeyFile));
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(text);
                byte[] encryptedBytes = rsa.Encrypt(inputBytes, true);
                string encryptedText = Convert.ToBase64String(encryptedBytes);
                return Json(new { success = true, encryptedText });
            }
        }

        [HttpPost]
        public IActionResult Decrypt(string encryptedText)
        {
            if (!System.IO.File.Exists(PrivateKeyFile))
                return Json(new { success = false, message = "Không tìm thấy khóa riêng tư!" });

            using (var rsa = new System.Security.Cryptography.RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                rsa.FromXmlString(System.IO.File.ReadAllText(PrivateKeyFile));
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] decryptedBytes = rsa.Decrypt(encryptedBytes, true);
                string decryptedText = System.Text.Encoding.UTF8.GetString(decryptedBytes);
                return Json(new { success = true, decryptedText });
            }
        }

        [HttpPost]
        public IActionResult Sign(string text)
        {
            if (!System.IO.File.Exists(PrivateKeyFile))
                return Json(new { success = false, message = "Không tìm thấy khóa riêng tư!" });

            using (var rsa = new System.Security.Cryptography.RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                rsa.FromXmlString(System.IO.File.ReadAllText(PrivateKeyFile));
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(text);
                byte[] signatureBytes = rsa.SignData(inputBytes, System.Security.Cryptography.SHA256.Create());
                string signature = Convert.ToBase64String(signatureBytes);
                return Json(new { success = true, signature });
            }
        }

        [HttpPost]
        public IActionResult VerifySignature(string text, string signature)
        {
            if (!System.IO.File.Exists(PublicKeyFile))
                return Json(new { success = false, message = "Không tìm thấy khóa công khai!" });

            using (var rsa = new System.Security.Cryptography.RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                rsa.FromXmlString(System.IO.File.ReadAllText(PublicKeyFile));
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(text);
                byte[] signatureBytes = Convert.FromBase64String(signature);
                bool isVerified = rsa.VerifyData(inputBytes, System.Security.Cryptography.SHA256.Create(), signatureBytes);
                return Json(new { success = true, isVerified });
            }
        }
    }
}