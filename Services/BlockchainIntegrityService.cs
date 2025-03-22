using Quanlydiem.Interfaces;
using Quanlydiem.Models;
using System.Collections.Generic;
using System.Linq;

namespace Quanlydiem.Services
{
    public class BlockchainIntegrityService
    {
        private readonly BlockChain _blockchain;

        public BlockchainIntegrityService(BlockChain blockchain)
        {
            _blockchain = blockchain;
        }

        /// <summary>
        /// Kiểm tra tính toàn vẹn của toàn bộ blockchain
        /// </summary>
        /// <returns>Danh sách các block bị sửa đổi</returns>
        public List<BlockIntegrityResult> VerifyBlockchainIntegrity()
        {
            var results = new List<BlockIntegrityResult>();
            
            foreach (var block in _blockchain.Blocks)
            {
                var integrityResult = VerifyBlockIntegrity(block);
                if (!integrityResult.IsValid)
                {
                    results.Add(integrityResult);
                }
            }
            
            return results;
        }

        /// <summary>
        /// Kiểm tra tính toàn vẹn của một block
        /// </summary>
        /// <param name="block">Block cần kiểm tra</param>
        /// <returns>Kết quả kiểm tra</returns>
        public BlockIntegrityResult VerifyBlockIntegrity(IBlock block)
        {
            // Tính toán lại Merkle root từ các giao dịch hiện tại
            var recalculatedMerkleRoot = block.CalculateMerkleRoot();
            // So sánh với Merkle root đã lưu trong block
            bool isValid = recalculatedMerkleRoot == block.MerkleRoot;
            
            return new BlockIntegrityResult
            {
                BlockNumber = block.BlockNumber,
                StoredMerkleRoot = block.MerkleRoot, // Merkle root gốc đã lưu
                CalculatedMerkleRoot = recalculatedMerkleRoot, // Merkle root tính lại
                IsValid = isValid,
                CanDetectSpecificTransaction = false, // Mặc định không thể xác định chính xác
                ModifiedTransactions = isValid ? new List<int>() : new List<int>() // Danh sách rỗng nếu không thể xác định chính xác
            };
        }

        /// <summary>
        /// Tìm các giao dịch bị sửa đổi trong một block
        /// </summary>
        /// <param name="block">Block cần kiểm tra</param>
        /// <returns>Danh sách index của các giao dịch bị sửa đổi (hoặc danh sách rỗng nếu không thể xác định)</returns>
        private List<int> FindModifiedTransactions(IBlock block)
        {
            // Trong triển khai hiện tại, chúng ta không lưu trữ hash ban đầu của mỗi giao dịch
            // nên không thể biết chính xác giao dịch nào bị sửa đổi
            // Trả về danh sách rỗng để biểu thị rằng không thể xác định chính xác
            return new List<int>();
        }
    }

    public class BlockIntegrityResult
    {
        public int BlockNumber { get; set; }
        public string StoredMerkleRoot { get; set; }
        public string CalculatedMerkleRoot { get; set; }
        public bool IsValid { get; set; }
        /// <summary>
        /// Cho biết có thể xác định chính xác giao dịch nào bị sửa đổi hay không
        /// </summary>
        public bool CanDetectSpecificTransaction { get; set; }
        public List<int> ModifiedTransactions { get; set; } = new List<int>();
    }
} 