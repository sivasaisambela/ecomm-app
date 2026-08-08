using AdminService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminService.Infrastructure.Persistence
{
    public class AdminDbContext : DbContext
    {
        public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options)
        {
        }

        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("AuditLogs");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Action)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.PerformedBy)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.EntityType)
                    .HasMaxLength(200);

                entity.Property(x => x.EntityId)
                    .HasMaxLength(200);

                entity.Property(x => x.Details)
                    .HasMaxLength(2000);

                entity.Property(x => x.CreatedAt)
                    .IsRequired();
            });
        }
    }
}
