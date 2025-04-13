using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Text.Json;

namespace blockchain_ClaimChain.Controllers
{
    public class FileEncryptionController : Controller
    {
        private readonly string PublicKeyFile = "wwwroot/publicKey.xml";
        private readonly string PrivateKeyFile = "wwwroot/privateKey.xml";
        private readonly string TempFilesFolder = "wwwroot/temp";

        public FileEncryptionController()
        {
            // Đảm bảo thư mục tạm tồn tại
            if (!Directory.Exists(TempFilesFolder))
                Directory.CreateDirectory(TempFilesFolder);
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult EncryptFile(IFormFile file, string publicKey)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Vui lòng chọn file để mã hóa!" });

            // Kiểm tra kích thước file
            if (file.Length > 100 * 1024) // Giới hạn 100KB
                return Json(new { success = false, message = "File quá lớn! RSA chỉ có thể mã hóa file nhỏ (tối đa 100KB)." });

            try
            {
                // Đọc file vào mảng byte
                byte[] fileBytes;
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    fileBytes = ms.ToArray();
                }

                // Tạo mã hash của file gốc
                string fileHash;
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(fileBytes);
                    fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }

                // Lấy phần mở rộng gốc của file
                string originalExtension = Path.GetExtension(file.FileName);
                
                // Tạo metadata lưu thông tin file gốc
                var fileMetadata = new 
                {
                    OriginalExtension = originalExtension,
                    OriginalFileName = Path.GetFileNameWithoutExtension(file.FileName)
                };
                
                // Chuyển metadata thành JSON
                string metadataJson = JsonSerializer.Serialize(fileMetadata);
                byte[] metadataBytes = Encoding.UTF8.GetBytes(metadataJson);
                
                // Thêm độ dài metadata vào đầu file để khi giải mã biết đọc bao nhiêu byte
                byte[] metadataLengthBytes = BitConverter.GetBytes(metadataBytes.Length);
                
                // Kết hợp độ dài metadata + metadata + dữ liệu file
                byte[] combinedBytes = new byte[4 + metadataBytes.Length + fileBytes.Length];
                Array.Copy(metadataLengthBytes, 0, combinedBytes, 0, 4);
                Array.Copy(metadataBytes, 0, combinedBytes, 4, metadataBytes.Length);
                Array.Copy(fileBytes, 0, combinedBytes, 4 + metadataBytes.Length, fileBytes.Length);

                // Mã hóa dữ liệu file
                byte[] encryptedBytes;
                using (var rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.PersistKeyInCsp = false;
                    
                    // Sử dụng khóa công khai được cung cấp hoặc từ file nếu không có
                    if (!string.IsNullOrWhiteSpace(publicKey))
                    {
                        rsa.FromXmlString(publicKey);
                    }
                    else
                    {
                        if (!System.IO.File.Exists(PublicKeyFile))
                            return Json(new { success = false, message = "Không tìm thấy khóa công khai! Vui lòng nhập khóa hoặc tạo cặp khóa mới." });
                        
                        rsa.FromXmlString(System.IO.File.ReadAllText(PublicKeyFile));
                    }

                    // Do giới hạn của RSA, chúng ta cần chia nhỏ dữ liệu
                    int maxBlockSize = 214; // Kích thước tối đa cho RSA 2048-bit với padding OAEP
                    var encryptedBlocks = new List<byte>();

                    for (int i = 0; i < combinedBytes.Length; i += maxBlockSize)
                    {
                        int blockSize = Math.Min(maxBlockSize, combinedBytes.Length - i);
                        byte[] dataBlock = new byte[blockSize];
                        Array.Copy(combinedBytes, i, dataBlock, 0, blockSize);

                        byte[] encryptedBlock = rsa.Encrypt(dataBlock, true);
                        encryptedBlocks.AddRange(encryptedBlock);
                    }

                    encryptedBytes = encryptedBlocks.ToArray();
                }

                // Lưu file mã hóa vào thư mục tạm và tạo URL để download
                string encryptedFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_encrypted.bin";
                string filePath = Path.Combine(TempFilesFolder, encryptedFileName);
                System.IO.File.WriteAllBytes(filePath, encryptedBytes);

                // Trả về URL để tải file và mã hash
                return Json(new 
                { 
                    success = true, 
                    fileName = encryptedFileName,
                    fileUrl = $"/temp/{encryptedFileName}",
                    fileHash = fileHash,
                    originalExtension = originalExtension
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi mã hóa file: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult DecryptFile(IFormFile file, string fileHash, string privateKey)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Vui lòng chọn file để giải mã!" });

            try
            {
                // Đọc file vào mảng byte
                byte[] encryptedBytes;
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    encryptedBytes = ms.ToArray();
                }

                // Giải mã dữ liệu
                List<byte> decryptedData = new List<byte>();
                using (var rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.PersistKeyInCsp = false;
                    
                    // Sử dụng khóa riêng tư được cung cấp hoặc từ file nếu không có
                    if (!string.IsNullOrWhiteSpace(privateKey))
                    {
                        rsa.FromXmlString(privateKey);
                    }
                    else
                    {
                        if (!System.IO.File.Exists(PrivateKeyFile))
                            return Json(new { success = false, message = "Không tìm thấy khóa riêng tư! Vui lòng nhập khóa hoặc tạo cặp khóa mới." });
                        
                        rsa.FromXmlString(System.IO.File.ReadAllText(PrivateKeyFile));
                    }

                    int blockSize = 256; // Kích thước block mã hóa RSA 2048-bit
                    for (int i = 0; i < encryptedBytes.Length; i += blockSize)
                    {
                        int currentBlockSize = Math.Min(blockSize, encryptedBytes.Length - i);
                        if (currentBlockSize < blockSize) break; // Đảm bảo block đủ dài

                        byte[] encryptedBlock = new byte[currentBlockSize];
                        Array.Copy(encryptedBytes, i, encryptedBlock, 0, currentBlockSize);

                        byte[] decryptedBlock = rsa.Decrypt(encryptedBlock, true);
                        decryptedData.AddRange(decryptedBlock);
                    }
                }

                // Chuyển danh sách byte thành mảng
                byte[] combinedBytes = decryptedData.ToArray();
                
                // Đọc độ dài metadata (4 byte đầu tiên)
                int metadataLength = BitConverter.ToInt32(combinedBytes, 0);
                
                // Đọc metadata
                string metadataJson = Encoding.UTF8.GetString(combinedBytes, 4, metadataLength);
                var fileMetadata = JsonSerializer.Deserialize<dynamic>(metadataJson);
                
                // Lấy phần mở rộng gốc
                string originalExtension = fileMetadata.GetProperty("OriginalExtension").GetString();
                string originalFileName = fileMetadata.GetProperty("OriginalFileName").GetString();
                
                // Lấy dữ liệu file thực tế
                byte[] fileBytes = new byte[combinedBytes.Length - 4 - metadataLength];
                Array.Copy(combinedBytes, 4 + metadataLength, fileBytes, 0, fileBytes.Length);

                // Tạo mã hash của file giải mã để kiểm tra tính toàn vẹn
                bool isHashValid = false;

                if (!string.IsNullOrEmpty(fileHash))
                {
                    // Tính mã hash của file giải mã
                    using (SHA256 sha256 = SHA256.Create())
                    {
                        byte[] hashBytes = sha256.ComputeHash(fileBytes);
                        string decryptedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                        isHashValid = decryptedHash.Equals(fileHash, StringComparison.OrdinalIgnoreCase);
                    }
                }

                // Lưu file giải mã vào thư mục tạm và tạo URL để download với đúng định dạng gốc
                string decryptedFileName = $"{originalFileName}{originalExtension}";
                string filePath = Path.Combine(TempFilesFolder, decryptedFileName);
                System.IO.File.WriteAllBytes(filePath, fileBytes);

                // Trả về URL để tải file và trạng thái xác thực hash
                return Json(new 
                { 
                    success = true, 
                    fileName = decryptedFileName,
                    fileUrl = $"/temp/{decryptedFileName}",
                    hashVerified = string.IsNullOrEmpty(fileHash) ? (bool?)null : isHashValid
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi giải mã file: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult GetPublicKey()
        {
            if (!System.IO.File.Exists(PublicKeyFile))
                return Json(new { success = false, message = "Không tìm thấy khóa công khai!" });

            try
            {
                string publicKey = System.IO.File.ReadAllText(PublicKeyFile);
                return Json(new { success = true, publicKey });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi đọc khóa công khai: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult GetPrivateKey()
        {
            if (!System.IO.File.Exists(PrivateKeyFile))
                return Json(new { success = false, message = "Không tìm thấy khóa riêng tư!" });

            try
            {
                string privateKey = System.IO.File.ReadAllText(PrivateKeyFile);
                return Json(new { success = true, privateKey });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi đọc khóa riêng tư: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult SignFile(IFormFile file, string privateKey)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Vui lòng chọn file để ký!" });

            try
            {
                // Đọc file vào mảng byte
                byte[] fileBytes;
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    fileBytes = ms.ToArray();
                }

                // Tạo chữ ký
                byte[] signatureBytes;
                using (var rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.PersistKeyInCsp = false;
                    
                    // Sử dụng khóa riêng tư được cung cấp hoặc từ file nếu không có
                    if (!string.IsNullOrWhiteSpace(privateKey))
                    {
                        rsa.FromXmlString(privateKey);
                    }
                    else
                    {
                        if (!System.IO.File.Exists(PrivateKeyFile))
                            return Json(new { success = false, message = "Không tìm thấy khóa riêng tư! Vui lòng nhập khóa hoặc tạo cặp khóa mới." });
                        
                        rsa.FromXmlString(System.IO.File.ReadAllText(PrivateKeyFile));
                    }
                    
                    signatureBytes = rsa.SignData(fileBytes, SHA256.Create());
                }

                // Chuyển đổi chữ ký sang chuỗi Base64
                string signature = Convert.ToBase64String(signatureBytes);

                return Json(new { success = true, signature });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi ký file: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult VerifyFileSignature(IFormFile file, string signature, string publicKey)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Vui lòng chọn file để xác thực!" });

            if (string.IsNullOrEmpty(signature))
                return Json(new { success = false, message = "Vui lòng nhập chữ ký!" });

            try
            {
                // Đọc file vào mảng byte
                byte[] fileBytes;
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    fileBytes = ms.ToArray();
                }

                // Chuyển đổi chữ ký từ chuỗi Base64 sang mảng byte
                byte[] signatureBytes = Convert.FromBase64String(signature);

                // Xác thực chữ ký
                bool isVerified;
                using (var rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.PersistKeyInCsp = false;
                    
                    // Sử dụng khóa công khai được cung cấp hoặc từ file nếu không có
                    if (!string.IsNullOrWhiteSpace(publicKey))
                    {
                        rsa.FromXmlString(publicKey);
                    }
                    else
                    {
                        if (!System.IO.File.Exists(PublicKeyFile))
                            return Json(new { success = false, message = "Không tìm thấy khóa công khai! Vui lòng nhập khóa hoặc tạo cặp khóa mới." });
                        
                        rsa.FromXmlString(System.IO.File.ReadAllText(PublicKeyFile));
                    }
                    
                    isVerified = rsa.VerifyData(fileBytes, SHA256.Create(), signatureBytes);
                }

                return Json(new { success = true, isVerified });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi xác thực chữ ký file: {ex.Message}" });
            }
        }
    }
} 