using Microsoft.AspNetCore.Http;
using Quanlydiem.Interfaces;
using Quanlydiem.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Quanlydiem.Services
{
    public class ImageService
    {
        private readonly BlockchainService _blockchainService;

        public ImageService(BlockchainService blockchainService)
        {
            _blockchainService = blockchainService;
        }

        /// <summary>
        /// Chuyển đổi file ảnh thành chuỗi Base64
        /// </summary>
        /// <param name="file">File ảnh</param>
        /// <returns>Chuỗi Base64 của ảnh</returns>
        public async Task<string> ConvertImageToBase64Async(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
        }
        
        /// <summary>
        /// Lấy MIME type của dữ liệu Base64
        /// </summary>
        /// <param name="base64">Chuỗi Base64 của ảnh</param>
        /// <returns>MIME type của ảnh (ví dụ: image/jpeg, image/png, ...)</returns>
        public string GetMimeType(string base64)
        {
            if (string.IsNullOrEmpty(base64))
                return null;
                
            // Một số signature phổ biến của file ảnh
            if (base64.StartsWith("/9j/"))
                return "image/jpeg";
            if (base64.StartsWith("iVBORw0Kg"))
                return "image/png";
            if (base64.StartsWith("R0lGODlh"))
                return "image/gif";
            if (base64.StartsWith("UklGR"))
                return "image/webp";
                
            // Mặc định
            return "image/jpeg";
        }
        
        /// <summary>
        /// Tạo HTML để hiển thị ảnh từ chuỗi Base64
        /// </summary>
        /// <param name="base64">Chuỗi Base64 của ảnh</param>
        /// <returns>Thẻ HTML img với src là dữ liệu Base64</returns>
        public string CreateImageTag(string base64)
        {
            if (string.IsNullOrEmpty(base64))
                return null;
                
            string mimeType = GetMimeType(base64);
            return $"<img src='data:{mimeType};base64,{base64}' class='img-fluid' alt='Blockchain Image' />";
        }
        
        /// <summary>
        /// Tạo một transaction mới chứa ảnh
        /// </summary>
        /// <param name="masinhvien">Mã sinh viên</param>
        /// <param name="mamonhoc">Mã môn học</param>
        /// <param name="imageBase64">Chuỗi Base64 của ảnh</param>
        /// <returns>Transaction đã được thêm vào blockchain</returns>
        public ITransaction CreateImageTransaction(string masinhvien, string mamonhoc, string imageBase64)
        {
            if (string.IsNullOrEmpty(imageBase64))
                throw new ArgumentException("Dữ liệu ảnh không được để trống");
                
            var transaction = new Transaction
            {
                masinhvien = masinhvien,
                mamonhoc = mamonhoc,
                diem = 0, // Không sử dụng điểm cho transaction ảnh
                ngayluudiem = DateTime.UtcNow,
                diemlanthu = 0, // Không sử dụng lần thứ cho transaction ảnh
                ImageData = imageBase64,
                HasImage = true
            };
            
            // Thêm transaction vào blockchain
            _blockchainService.AddTransaction(transaction);
            
            return transaction;
        }
    }
} 