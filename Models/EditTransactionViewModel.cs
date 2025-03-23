using System;
using System.ComponentModel.DataAnnotations;

namespace Quanlydiem.Models
{
    public class EditTransactionViewModel
    {
        public int BlockNumber { get; set; }
        public int TransactionId { get; set; }

        // THUỘC TÍNH 1: Có thể thay đổi MaSinhVien và các thông báo lỗi liên quan
        [Required(ErrorMessage = "Mã sinh viên không được để trống")]
        [Display(Name = "Mã sinh viên")]
        public string MaSinhVien { get; set; }

        // THUỘC TÍNH 2: Có thể thay đổi MaMonHoc và các thông báo lỗi liên quan
        [Required(ErrorMessage = "Mã môn học không được để trống")]
        [Display(Name = "Mã môn học")]
        public string MaMonHoc { get; set; }

        // THUỘC TÍNH 3: Có thể thay đổi Diem và các thông báo lỗi liên quan
        [Required(ErrorMessage = "Điểm không được để trống")]
        [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
        [Display(Name = "Điểm")]
        public int Diem { get; set; }

        // THUỘC TÍNH 4: Có thể thay đổi NgayLuuDiem và các thông báo lỗi liên quan
        [Required(ErrorMessage = "Ngày lưu điểm không được để trống")]
        [Display(Name = "Ngày lưu điểm")]
        public DateTime NgayLuuDiem { get; set; }

        // THUỘC TÍNH 5: Có thể thay đổi DiemLanThu và các thông báo lỗi liên quan
        [Required(ErrorMessage = "Điểm lần thứ không được để trống")]
        [Range(1, 10, ErrorMessage = "Điểm lần thứ phải từ 1 đến 10")]
        [Display(Name = "Điểm lần thứ")]
        public int DiemLanThu { get; set; }
        
        // Thuộc tính mới để lưu trữ dữ liệu ảnh dưới dạng Base64
        [Display(Name = "Dữ liệu ảnh")]
        public string? ImageData { get; set; }
        
        // Thuộc tính để đánh dấu có ảnh hay không
        [Display(Name = "Có ảnh")]
        public bool HasImage { get; set; }
    }
} 