using System;
using System.IO;
using ErogeHelper.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace ErogeHelper.Repository.Data
{
    public class EHCacheContext : DbContext
    {
        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            var file = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData) + @"\ErogeHelper\eh_cache.db");
            file = Path.GetFullPath(file);

            optionsBuilder.UseSqlite(
                $"Filename={file}");
            // optionsBuilder.UseLazyLoadingProxies();
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>()
                .HasMany(it => it.Names)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<Subtitle> Comments => Set<Subtitle>();
        public DbSet<Game> Games => Set<Game>();
        public DbSet<User> Users => Set<User>();
    }
}