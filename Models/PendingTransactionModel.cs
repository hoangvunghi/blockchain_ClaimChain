using Quanlydiem.Interfaces; 
using Quanlydiem;
using System;
using System.Text;
namespace Quanlydiem.Models
{
    using System;

    public class PendingTransactionModel: ITransaction
    {
        // id
        public int Id { get; set; }
        // THUỘC TÍNH 1: Có thể thay đổi tochuccap
        public string tochuccap { get; set; }
        // THUỘC TÍNH 2: Có thể thay đổi hoten
        public string hoten { get; set; }
        // THUỘC TÍNH 3: Có thể thay đổi diem
        public string socancuoc { get; set; }
        public string TransactionHash { get; set; } 
        // THUỘC TÍNH 4: Có thể thay đổi ngaycap
        public DateTime ngaycap { get; set; }
        // THUỘC TÍNH 5: Có thể thay đổi diemlanthu
        public LoaiTotNghiep loaitotnghiep { get; set; }
        public string CalculateTransactionHash()
        {
            // LƯU Ý: Nếu thay đổi tên các thuộc tính, cần cập nhật cả dòng này
            string txnData = tochuccap + hoten + socancuoc + ngaycap + loaitotnghiep;
            return Convert.ToBase64String(Hashing.ComputeHashSha256(Encoding.UTF8.GetBytes(txnData)));
        }
    }
}
