namespace Quanlydiem.Interfaces 
{
    public enum LoaiTotNghiep
    {
        XuatSac,
        Gioi,
        Kha,
        Trungbinh
    }
    public interface ITransaction
    {
        // THUỘC TÍNH 1: Có thể thay đổi tochuccap
        string tochuccap { get; set; }
        // THUỘC TÍNH 2: Có thể thay đổi hoten
        string hoten { get; set; }
        // THUỘC TÍNH 3: Có thể thay đổi diem
        string socancuoc { get; set; }
        // THUỘC TÍNH 4: Có thể thay đổi ngaycap
        DateTime ngaycap { get; set; }
        // THUỘC TÍNH 5: Có thể thay đổi diemlanthu
        LoaiTotNghiep loaitotnghiep{set;get;}
        string CalculateTransactionHash();
    }
}