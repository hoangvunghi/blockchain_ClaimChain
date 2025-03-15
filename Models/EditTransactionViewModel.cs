using System;
using System.ComponentModel.DataAnnotations;

namespace Quanlydiem.Models
{
    public class EditTransactionViewModel
    {
        public int BlockNumber { get; set; }
        public int TransactionId { get; set; }

        [Required(ErrorMessage = "Mã sinh viên không được để trống")]
        [Display(Name = "Mã sinh viên")]
        public string MaSinhVien { get; set; }

        [Required(ErrorMessage = "Mã môn học không được để trống")]
        [Display(Name = "Mã môn học")]
        public string MaMonHoc { get; set; }

        [Required(ErrorMessage = "Điểm không được để trống")]
        [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
        [Display(Name = "Điểm")]
        public int Diem { get; set; }

        [Required(ErrorMessage = "Ngày lưu điểm không được để trống")]
        [Display(Name = "Ngày lưu điểm")]
        public DateTime NgayLuuDiem { get; set; }

        [Required(ErrorMessage = "Điểm lần thứ không được để trống")]
        [Range(1, 10, ErrorMessage = "Điểm lần thứ phải từ 1 đến 10")]
        [Display(Name = "Điểm lần thứ")]
        public int DiemLanThu { get; set; }
    }
} 