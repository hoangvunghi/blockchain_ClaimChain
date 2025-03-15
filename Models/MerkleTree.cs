using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quanlydiem.Models
{
    public class MerkleTree
    {
        public string RootHash { get; private set; }
        private List<string> leafHashes;

        public MerkleTree(List<string> transactionHashes)
        {
            if (transactionHashes == null || !transactionHashes.Any())
            {
                leafHashes = new List<string>();
                RootHash = string.Empty;
                return;
            }

            leafHashes = new List<string>(transactionHashes);
            RootHash = BuildMerkleTree(leafHashes);
        }

        public static string BuildMerkleTree(List<string> hashes)
        {
            if (hashes == null || !hashes.Any())
                return string.Empty;

            if (hashes.Count == 1)
                return hashes[0];

            var newHashes = new List<string>();

            for (int i = 0; i < hashes.Count; i += 2)
            {
                if (i + 1 < hashes.Count)
                {
                    // Nếu có đủ 2 hash, kết hợp chúng lại và tính hash mới
                    string combinedHash = hashes[i] + hashes[i + 1];
                    newHashes.Add(Hashing.CalculateHash(combinedHash));
                }
                else
                {
                    // Nếu chỉ còn 1 hash, sử dụng nó làm hash mới
                    newHashes.Add(hashes[i]);
                }
            }

            // Đệ quy cho đến khi chỉ còn 1 hash (root hash)
            return BuildMerkleTree(newHashes);
        }

        public bool VerifyTransaction(string transactionHash, List<string> proof, int index)
        {
            string computedHash = transactionHash;

            foreach (string proofElement in proof)
            {
                if (index % 2 == 0)
                {
                    // Nếu index chẵn, hash = Hash(current + proof)
                    computedHash = Hashing.CalculateHash(computedHash + proofElement);
                }
                else
                {
                    // Nếu index lẻ, hash = Hash(proof + current)
                    computedHash = Hashing.CalculateHash(proofElement + computedHash);
                }
                index /= 2;
            }

            // Nếu hash cuối cùng bằng root hash, transaction hợp lệ
            return computedHash == RootHash;
        }

        public List<string> GenerateProof(string transactionHash)
        {
            int index = leafHashes.IndexOf(transactionHash);
            if (index == -1)
                return new List<string>();

            return GenerateProof(index);
        }

        private List<string> GenerateProof(int index)
        {
            var proof = new List<string>();
            var currentHashes = new List<string>(leafHashes);

            while (currentHashes.Count > 1)
            {
                var newHashes = new List<string>();

                for (int i = 0; i < currentHashes.Count; i += 2)
                {
                    if (i + 1 < currentHashes.Count)
                    {
                        newHashes.Add(Hashing.CalculateHash(currentHashes[i] + currentHashes[i + 1]));
                    }
                    else
                    {
                        newHashes.Add(currentHashes[i]);
                    }
                }

                // Thêm hash cần thiết vào proof
                if (index % 2 == 0 && index + 1 < currentHashes.Count)
                {
                    proof.Add(currentHashes[index + 1]);
                }
                else if (index % 2 == 1)
                {
                    proof.Add(currentHashes[index - 1]);
                }

                currentHashes = newHashes;
                index /= 2;
            }

            return proof;
        }
    }
} 