using Microsoft.AspNetCore.Mvc;
using Quanlydiem.Interfaces;
using Quanlydiem.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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
    }
}