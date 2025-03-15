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
        public string masinhvien { get; set; }
        public string mamonhoc { get; set; }
        public int diem { get; set; }
        public DateTime ngayluudiem { get; set; }
        public string TransactionHash { get; set; } 
        public int diemlanthu { get; set; }
       
    }
}
