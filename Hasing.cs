using System.Security.Cryptography;
using System.Text;

namespace Quanlydiem
{
    public static class Hashing
    {
        public static byte[] ComputeHashSha256(byte[] data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(data);
            }
        }

        public static string CalculateHash(string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            byte[] hash = ComputeHashSha256(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}