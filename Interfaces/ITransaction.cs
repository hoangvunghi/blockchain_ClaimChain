namespace Quanlydiem.Interfaces 
{
    public interface ITransaction
    {
        string masinhvien { get; set; }
        string mamonhoc { get; set; }
        int diem { get; set; }
        DateTime ngayluudiem { get; set; }
        int diemlanthu{set;get;}
        string CalculateTransactionHash();
    }
}