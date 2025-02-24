﻿// <auto-generated />
using System;
using BlockchainTest.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BlockchainTest.Migrations
{
    [DbContext(typeof(BlockChainContext))]
    [Migration("20250224095551_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.2");

            modelBuilder.Entity("BlockchainTest.Models.BlockModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("BlockChainId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("BlockHash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("BlockNumber")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("NextBlockId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PreviousBlockHash")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("BlockHash")
                        .IsUnique();

                    b.HasIndex("BlockNumber")
                        .IsUnique();

                    b.HasIndex("NextBlockId");

                    b.ToTable("Blocks");
                });

            modelBuilder.Entity("BlockchainTest.Models.TransactionModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BlockId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CarRegistration")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ClaimNumber")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("ClaimType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Mileage")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("SettlementAmount")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("SettlementDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("TransactionHash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("BlockId");

                    b.HasIndex("TransactionHash")
                        .IsUnique();

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("BlockchainTest.Models.BlockModel", b =>
                {
                    b.HasOne("BlockchainTest.Models.BlockModel", "NextBlock")
                        .WithMany()
                        .HasForeignKey("NextBlockId");

                    b.Navigation("NextBlock");
                });

            modelBuilder.Entity("BlockchainTest.Models.TransactionModel", b =>
                {
                    b.HasOne("BlockchainTest.Models.BlockModel", "Block")
                        .WithMany("Transactions")
                        .HasForeignKey("BlockId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Block");
                });

            modelBuilder.Entity("BlockchainTest.Models.BlockModel", b =>
                {
                    b.Navigation("Transactions");
                });
#pragma warning restore 612, 618
        }
    }
}
