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
        public string masinhvien { get; set; }
        public string mamonhoc { get; set; }
        public int diem { get; set; }
        public string TransactionHash { get; set; } 
        public DateTime ngayluudiem { get; set; }
        public int diemlanthu { get; set; }
        public string CalculateTransactionHash()
        {
            string txnData = masinhvien + mamonhoc + diem + ngayluudiem + diemlanthu;
            return Convert.ToBase64String(Hashing.ComputeHashSha256(Encoding.UTF8.GetBytes(txnData)));
        }
    }
}
