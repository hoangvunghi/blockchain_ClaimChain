using System.Security.Cryptography;

namespace BlockchainTest
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
    }
}