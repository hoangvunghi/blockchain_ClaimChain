using BlockchainTest.Models;
using System.Collections.Generic;
using System;
using BlockchainTest.Interfaces;
using BlockchainTest.Models;
public class BlockchainService
{
    public BlockChain Blockchain { get; private set; }

    public BlockchainService()
    {
        Blockchain = new BlockChain();
        if (Blockchain.Blocks.Count == 0)
        {
            Blockchain.AddBlock(new List<ITransaction>());
            var transaction1 = new Transaction
            {
                ClaimNumber = "Claim001",
                SettlementAmount = 1500.00m,
                SettlementDate = DateTime.UtcNow,
                CarRegistration = "CAR-123",
                Mileage = 5000,
                ClaimType = ClaimType.Collision
            };
            Blockchain.AddBlock(new List<ITransaction>() { transaction1 });

            var transaction2 = new Transaction
            {
                ClaimNumber = "Claim002",
                SettlementAmount = 2500.00m,
                SettlementDate = DateTime.UtcNow,
                CarRegistration = "CAR-456",
                Mileage = 12000,
                ClaimType = ClaimType.Theft
            };
            Blockchain.AddBlock(new List<ITransaction>() { transaction2 });
        }
    }
}
