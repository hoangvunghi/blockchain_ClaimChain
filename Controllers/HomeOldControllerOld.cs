using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace BlockchainTest.Controllers
{
    public class HomeOldController : Controller
    {
        private readonly string PublicKeyFile = "wwwroot/publicKey.xml";
        private readonly string PrivateKeyFile = "wwwroot/privateKey.xml";

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GenerateKeys()
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                string publicKey = rsa.ToXmlString(false);
                string privateKey = rsa.ToXmlString(true);
                System.IO.File.WriteAllText(PublicKeyFile, publicKey);
                System.IO.File.WriteAllText(PrivateKeyFile, privateKey);
            }
            return Json(new { success = true, message = "Key pair generated successfully!" });
        }

        public IActionResult Encrypt(string text)
        {
            if (!System.IO.File.Exists(PublicKeyFile))
                return Json(new { success = false, message = "Public key not found!" });

            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                rsa.FromXmlString(System.IO.File.ReadAllText(PublicKeyFile));
                byte[] inputBytes = Encoding.UTF8.GetBytes(text);
                byte[] encryptedBytes = rsa.Encrypt(inputBytes, true);
                string encryptedText = Convert.ToBase64String(encryptedBytes);
                return Json(new { success = true, encryptedText });
            }
        }

        public IActionResult Decrypt(string encryptedText)
        {
            if (!System.IO.File.Exists(PrivateKeyFile))
                return Json(new { success = false, message = "Private key not found!" });

            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                rsa.FromXmlString(System.IO.File.ReadAllText(PrivateKeyFile));
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] decryptedBytes = rsa.Decrypt(encryptedBytes, true);
                string decryptedText = Encoding.UTF8.GetString(decryptedBytes);
                return Json(new { success = true, decryptedText });
            }
        }

        public IActionResult Sign(string text)
        {
            if (!System.IO.File.Exists(PrivateKeyFile))
                return Json(new { success = false, message = "Private key not found!" });

            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                rsa.FromXmlString(System.IO.File.ReadAllText(PrivateKeyFile));
                byte[] inputBytes = Encoding.UTF8.GetBytes(text);
                byte[] signatureBytes = rsa.SignData(inputBytes, SHA256.Create());
                string signature = Convert.ToBase64String(signatureBytes);
                return Json(new { success = true, signature });
            }
        }

        public IActionResult VerifySignature(string text, string signature)
        {
            if (!System.IO.File.Exists(PublicKeyFile))
                return Json(new { success = false, message = "Public key not found!" });

            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                rsa.FromXmlString(System.IO.File.ReadAllText(PublicKeyFile));
                byte[] inputBytes = Encoding.UTF8.GetBytes(text);
                byte[] signatureBytes = Convert.FromBase64String(signature);
                bool isVerified = rsa.VerifyData(inputBytes, SHA256.Create(), signatureBytes);
                return Json(new { success = true, isVerified });
            }
        }
    }
}
