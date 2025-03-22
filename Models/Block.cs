namespace Quanlydiem.Models
{
    using Quanlydiem.Interfaces;
    using System.Collections.Generic;
    using System.Text;
    using System;
    using System.Security.Cryptography;
    using System.Linq;

    public class Block : IBlock
    {
        public List<ITransaction> Transactions { get; set; } = new List<ITransaction>();
        public int BlockNumber { get; private set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string BlockHash { get; set; }
        public string PreviousBlockHash { get; set; }
        public string MerkleRoot { get; set; }
        public IBlock NextBlock { get; set; }
        private List<string> OriginalTransactionHashes { get; set; } = new List<string>();

        public Block(int blockNumber)
        {
            BlockNumber = blockNumber;
            CreatedDate = DateTime.UtcNow;
        }

        public void AddTransaction(ITransaction transaction)
        {
            Transactions.Add(transaction);
            OriginalTransactionHashes.Add(transaction.CalculateTransactionHash());
        }

        public string CalculateMerkleRoot()
        {
            var transactionHashes = Transactions.Select(t => t.CalculateTransactionHash()).ToList();
            return MerkleTree.BuildMerkleTree(transactionHashes);
        }

        public string CalculateBlockHash()
        {
            if (string.IsNullOrEmpty(MerkleRoot))
            {
                MerkleRoot = CalculateMerkleRoot();
            }
            
            string blockData = System.Text.Json.JsonSerializer.Serialize(new
            {
                MerkleRoot,
                BlockNumber,
                CreatedDate,
                PreviousBlockHash
            });
            return Convert.ToBase64String(Hashing.ComputeHashSha256(Encoding.UTF8.GetBytes(blockData)));
        }

        public void SetBlockHash(IBlock parent)
        {
            if (parent != null)
            {
                PreviousBlockHash = parent.BlockHash;
                parent.NextBlock = this;
            }
            else
            {
                PreviousBlockHash = null;
            }

            MerkleRoot = CalculateMerkleRoot();
            BlockHash = CalculateBlockHash();
        }

        public bool IsValidChain(string previousBlockHash, bool verbose)
        {
            bool isValid = true;

            if (PreviousBlockHash != previousBlockHash)
            {
                isValid = false;
                if (verbose)
                {
                    Console.WriteLine($"Block #{BlockNumber}: PreviousBlockHash không khớp.");
                }
            }
            
            string recalculatedMerkleRoot = CalculateMerkleRoot();
            
            if (MerkleRoot != recalculatedMerkleRoot)
            {
                isValid = false;
                if (verbose)
                {
                    Console.WriteLine($"Block #{BlockNumber}: MerkleRoot không khớp. Dữ liệu giao dịch đã bị sửa đổi.");
                }
            }
            
            string recomputedHash = CalculateBlockHash();
            if (BlockHash != recomputedHash)
            {
                isValid = false;
                if (verbose)
                {
                    Console.WriteLine($"Block #{BlockNumber}: BlockHash không khớp.");
                }
            }

            if (verbose && isValid)
            {
                Console.WriteLine($"Block #{BlockNumber}: Hợp lệ.");
            }
            if (NextBlock != null)
            {
                return NextBlock.IsValidChain(BlockHash, verbose);
            }
            return isValid;
        }
    }
}