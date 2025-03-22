namespace Quanlydiem.Models 
{
    using System.Security.Cryptography;
    // import để sử dụng convert
    using System;
    using System.Text;
    using Quanlydiem;
    using Quanlydiem.Interfaces; 
    public class Transaction : ITransaction
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
        // THUỘC TÍNH 5: Có thể thay đổi diemlanthu
        public int diemlanthu { get; set; }
        public string CalculateTransactionHash()
        {
            // LƯU Ý: Nếu thay đổi tên các thuộc tính, cần cập nhật cả dòng này
            string txnData = masinhvien + mamonhoc + diem + ngayluudiem + diemlanthu;
            return Convert.ToBase64String(Hashing.ComputeHashSha256(Encoding.UTF8.GetBytes(txnData)));
        }
    }
}
