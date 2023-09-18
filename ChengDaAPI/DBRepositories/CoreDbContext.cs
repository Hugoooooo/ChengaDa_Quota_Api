using System;
using ChengDaApi.DBRepositories.DBSchema;
using Microsoft.EntityFrameworkCore;

namespace ChengDaApi.DBRepositories
{
    public class CoreDbContext : DbContext
    {
        public CoreDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Quote>()
                .HasKey(item => new { item.id });
            modelBuilder.Entity<DayoffDetail>()
                .HasKey(item => new { item.id });
            modelBuilder.Entity<QuoteDetail>()
                .HasKey(item => new { item.id });
            modelBuilder.Entity<SystemParameter>()
                .HasKey(item => new { item.id });
            modelBuilder.Entity<Member>()
                .HasKey(item => new { item.id });
            modelBuilder.Entity<PunchDetail>()
                .HasKey(item => new { item.id });
        }

        public virtual DbSet<Quote> Quote { set; get; }
        public virtual DbSet<DayoffDetail> DayoffDetail { set; get; }
        public virtual DbSet<QuoteDetail> QuoteDetail { set; get; }
        public virtual DbSet<SystemParameter> SystemParameter { set; get; }
        public virtual DbSet<Member> Member { set; get; }
        public virtual DbSet<PunchDetail> PunchDetail { set; get; }
        public virtual DbSet<Account> Account { set; get; }
        public virtual DbSet<PurchaseOrder> PurchaseOrder { set; get; }
        public virtual DbSet<PurchaseDetail> PurchaseDetail { set; get; }
        public virtual DbSet<ShipOrder> ShipOrder { set; get; }
        public virtual DbSet<ShipDetail> ShipDetail { set; get; }
        public virtual DbSet<Inventory> Inventory { set; get; }

    }
}
