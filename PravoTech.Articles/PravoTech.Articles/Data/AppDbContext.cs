using Microsoft.EntityFrameworkCore;
using PravoTech.Articles.Entities;

namespace PravoTech.Articles.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Article> Articles { get; set; } = null!;

        public DbSet<Section> Sections { get; set; } = null!;

        public DbSet<Tag> Tags { get; set; } = null!;

        public DbSet<ArticleTag> ArticleTags { get; set; } = null!;

        public DbSet<SectionTag> SectionTags { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Article entity configuration
            modelBuilder.Entity<Article>(entity =>
            {
                // Configure ID generation using NEWSEQUENTIALID()
                entity.Property(a => a.Id)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("NEWSEQUENTIALID()");

                // Computed field EffectiveDate that uses UpdatedAt or CreatedAt
                entity.Property(a => a.EffectiveDate)
                    .HasComputedColumnSql("ISNULL([UpdatedAt], [CreatedAt])", stored: true);

                // Index for optimizing queries by EffectiveDate
                entity.HasIndex(a => a.EffectiveDate)
                    .HasDatabaseName("IX_Articles_EffectiveDate");
            });

            // Section entity configuration
            modelBuilder.Entity<Section>(entity =>
            {
                // Configure ID generation using NEWSEQUENTIALID()
                entity.Property(s => s.Id)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("NEWSEQUENTIALID()");
            });

            // ArticleTag entity configuration
            modelBuilder.Entity<ArticleTag>(entity =>
            {
                // Composite primary key from ArticleId and TagId
                entity.HasKey(at => new { at.ArticleId, at.TagId });

                // Unique index to maintain tag order in articles
                entity.HasIndex(at => new { at.ArticleId, at.Order })
                    .IsUnique();
            });

            // SectionTag entity configuration
            modelBuilder.Entity<SectionTag>(entity =>
            {
                // Composite primary key from SectionId and TagId
                entity.HasKey(at => new { at.SectionId, at.TagId });
            });

            // Tag entity configuration
            modelBuilder.Entity<Tag>(entity =>
            {
                // Unique index for normalized tag name
                entity.HasIndex(t => t.NormalizedName)
                    .IsUnique()
                    .HasDatabaseName("IX_Tags_NormalizedName");
            });
        }
    }
}
