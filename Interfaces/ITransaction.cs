namespace Quanlydiem.Interfaces 
{
    public interface ITransaction
    {
        // THUỘC TÍNH 1: Có thể thay đổi masinhvien
        string masinhvien { get; set; }
        // THUỘC TÍNH 2: Có thể thay đổi mamonhoc
        string mamonhoc { get; set; }
        // THUỘC TÍNH 3: Có thể thay đổi diem
        int diem { get; set; }
        // THUỘC TÍNH 4: Có thể thay đổi ngayluudiem
        DateTime ngayluudiem { get; set; }
        // THUỘC TÍNH 5: Có thể thay đổi diemlanthu
        int diemlanthu { set; get; }
        
        // Thuộc tính mới để lưu trữ dữ liệu ảnh dưới dạng Base64
        string? ImageData { get; set; }
        
        // Thuộc tính để đánh dấu giao dịch có chứa ảnh hay không
        bool HasImage { get; set; }
        
        string CalculateTransactionHash();
    }
}