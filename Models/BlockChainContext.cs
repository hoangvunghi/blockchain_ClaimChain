using Microsoft.EntityFrameworkCore;

namespace BlockchainTest.Models
{
    public class BlockChainContext : DbContext
    {
        public DbSet<BlockModel> Blocks { get; set; }
        public DbSet<TransactionModel> Transactions { get; set; }

        public BlockChainContext(DbContextOptions<BlockChainContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=blockchain.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BlockModel>()
        .HasIndex(b => b.BlockNumber)
        .IsUnique();
        
    modelBuilder.Entity<BlockModel>()
        .HasIndex(b => b.BlockHash)
        .IsUnique();

            // Đảm bảo BlockHash và TransactionHash là duy nhất
            modelBuilder.Entity<BlockModel>()
                .HasIndex(b => b.BlockHash)
                .IsUnique();

            modelBuilder.Entity<TransactionModel>()
                .HasIndex(t => t.TransactionHash)
                .IsUnique();
        }
    }
}