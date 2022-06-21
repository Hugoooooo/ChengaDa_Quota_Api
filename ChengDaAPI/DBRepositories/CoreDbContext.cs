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
            modelBuilder.Entity<QuoteDetail>()
                .HasKey(item => new { item.id });
        }

        public virtual DbSet<Quote> Quote { set; get; }
        public virtual DbSet<QuoteDetail> QuoteDetail { set; get; }



    }
}
