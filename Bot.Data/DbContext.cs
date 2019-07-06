using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Bot.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Bot.Data
{
    public class BotContext : DbContext
    {
        public DbSet<User> Users { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=Configs\\TimezoneBot.db", options =>
            {
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            });


            base.OnConfiguring(optionsBuilder);
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Map table names
            modelBuilder.Entity<User>().ToTable("DiscordUsers", "dbo");
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(x=> x.UserId);
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.Property(e => e.TimezoneId).HasDefaultValue(null);
            });


            base.OnModelCreating(modelBuilder);
        }
    }
}
