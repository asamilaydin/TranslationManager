using Microsoft.EntityFrameworkCore;
using TranslationManager.API.Models;

namespace TranslationManager.API.Data
{
    public class TranslationDbContext : DbContext
    {
        public TranslationDbContext(DbContextOptions<TranslationDbContext> options) : base(options)
        {
        }

        public DbSet<Translation> Translations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Table name mapping
            modelBuilder.Entity<Translation>()
                .ToTable("translations");

            // Column mappings
            modelBuilder.Entity<Translation>()
                .Property(t => t.Id)
                .HasColumnName("id");
            
            modelBuilder.Entity<Translation>()
                .Property(t => t.ResourceKey)
                .HasColumnName("resource_key");
            
            modelBuilder.Entity<Translation>()
                .Property(t => t.En)
                .HasColumnName("en");
            
            modelBuilder.Entity<Translation>()
                .Property(t => t.Tr)
                .HasColumnName("tr");
            
            modelBuilder.Entity<Translation>()
                .Property(t => t.De)
                .HasColumnName("de");
            
            modelBuilder.Entity<Translation>()
                .Property(t => t.Platform)
                .HasColumnName("platform");
            
            modelBuilder.Entity<Translation>()
                .Property(t => t.MobileSynced)
                .HasColumnName("mobile_synced");
            
            modelBuilder.Entity<Translation>()
                .Property(t => t.CreatedAt)
                .HasColumnName("created_at")
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            
            modelBuilder.Entity<Translation>()
                .Property(t => t.UpdatedAt)
                .HasColumnName("updated_at")
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            // ResourceKey unique constraint (sadece ResourceKey'e göre unique)
            modelBuilder.Entity<Translation>()
                .HasIndex(t => t.ResourceKey)
                .IsUnique();

            // Platform index
            modelBuilder.Entity<Translation>()
                .HasIndex(t => t.Platform);

            // UpdatedAt index
            modelBuilder.Entity<Translation>()
                .HasIndex(t => t.UpdatedAt);

            // No seed data: fresh databases should start empty
        }
    }
}
