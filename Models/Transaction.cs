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
        public string masinhvien { get; set; }
        public string mamonhoc { get; set; }
        public int diem { get; set; }
        public DateTime ngayluudiem { get; set; }
        public int diemlanthu { get; set; }
        public string CalculateTransactionHash()
        {
            string txnData = masinhvien + mamonhoc + diem + ngayluudiem + diemlanthu;
            return Convert.ToBase64String(Hashing.ComputeHashSha256(Encoding.UTF8.GetBytes(txnData)));
        }
    }
}
