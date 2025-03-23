using Quanlydiem.Interfaces;
using System.Security.Cryptography;
using System;
using System.Text;
using Quanlydiem;
namespace Quanlydiem.Models
{
    public class TransactionModel
    {
        public int Id { get; set; }
        // THUỘC TÍNH 1: Có thể thay đổi masinhvien
        public string masinhvien { get; set; }
        // THUỘC TÍNH 2: Có thể thay đổi mamonhoc
        public string mamonhoc { get; set; }
        // THUỘC TÍNH 3: Có thể thay đổi diem
        public int diem { get; set; }
        // THUỘC TÍNH 4: Có thể thay đổi ngayluudiem
        public DateTime ngayluudiem { get; set; }
        public string TransactionHash { get; set; } 
        // THUỘC TÍNH 5: Có thể thay đổi diemlanthu
        public int diemlanthu { get; set; }
        
        // Thuộc tính mới để lưu trữ dữ liệu ảnh dưới dạng Base64
        public string? ImageData { get; set; }
        
        // Thuộc tính để đánh dấu giao dịch có chứa ảnh hay không
        public bool HasImage { get; set; } = false;
    }
}
