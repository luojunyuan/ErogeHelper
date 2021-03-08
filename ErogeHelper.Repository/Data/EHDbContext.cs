using System;
using System.IO;
using ErogeHelper.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace ErogeHelper.Repository.Data
{
    public class EHDbContext : DbContext
    {
        public DbSet<Subtitle> Subtitles => Set<Subtitle>();

        public DbSet<Game> Games => Set<Game>();

        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            // TODO: if folder not exitst, then create one 
            var file = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData) + @"\ErogeHelper\eh.db");
            file = Path.GetFullPath(file);

            optionsBuilder.UseSqlite(
                $"Filename={file}");
            base.OnConfiguring(optionsBuilder);
        }

        // Migration command
        // C:/Program Files (x86)/dotnet/dotnet.exe" dotnet ef migrations add
    }
}