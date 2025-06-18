using Microsoft.EntityFrameworkCore;
using PravoTech.Articles.Constants;
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
                    .HasDefaultValueSql(DatabaseConstants.NewSequentialIdFunction);

                // Configure RowVersion for optimistic concurrency
                entity.Property(a => a.RowVersion)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                // Configure Title with proper constraints
                entity.Property(a => a.Title)
                    .IsRequired()
                    .HasMaxLength(ValidationConstants.MaxTitleLength);

                // Configure EffectiveDate
                entity.Property(a => a.EffectiveDate)
                    .IsRequired();

                // Index for sorting articles by effective date
                entity.HasIndex(a => a.EffectiveDate)
                    .HasDatabaseName(DatabaseConstants.ArticlesEffectiveDateIndex);
            });

            // Section entity configuration
            modelBuilder.Entity<Section>(entity =>
            {
                // Configure ID generation using NEWSEQUENTIALID()
                entity.Property(s => s.Id)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql(DatabaseConstants.NewSequentialIdFunction);

                // Configure Name with proper constraints
                entity.Property(s => s.Name)
                    .IsRequired()
                    .HasMaxLength(ValidationConstants.MaxSectionNameLength);
            });

            // Tag entity configuration
            modelBuilder.Entity<Tag>(entity =>
            {
                // Configure ID as identity
                entity.Property(t => t.Id)
                    .ValueGeneratedOnAdd();

                // Configure Name with proper constraints
                entity.Property(t => t.Name)
                    .IsRequired()
                    .HasMaxLength(ValidationConstants.MaxTagNameLength);

                // Configure NormalizedName with proper constraints
                entity.Property(t => t.NormalizedName)
                    .IsRequired()
                    .HasMaxLength(ValidationConstants.MaxTagNameLength);

                // Unique index for normalized tag name (used for lookups)
                entity.HasIndex(t => t.NormalizedName)
                    .IsUnique()
                    .HasDatabaseName(DatabaseConstants.TagsNormalizedNameIndex);
            });

            // ArticleTag entity configuration
            modelBuilder.Entity<ArticleTag>(entity =>
            {
                // Composite primary key from ArticleId and TagId
                entity.HasKey(at => new { at.ArticleId, at.TagId });

                // Configure Order
                entity.Property(at => at.Order)
                    .IsRequired();

                // Configure relationships
                entity.HasOne(at => at.Article)
                    .WithMany(a => a.ArticleTags)
                    .HasForeignKey(at => at.ArticleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(at => at.Tag)
                    .WithMany(t => t.ArticleTags)
                    .HasForeignKey(at => at.TagId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Unique index to maintain tag order in articles
                entity.HasIndex(at => new { at.ArticleId, at.Order })
                    .IsUnique()
                    .HasDatabaseName(DatabaseConstants.ArticleTagsOrderIndex);

                // Index for performance (used for tag lookups)
                entity.HasIndex(at => at.TagId)
                    .HasDatabaseName(DatabaseConstants.ArticleTagsTagIdIndex);
            });

            // SectionTag entity configuration
            modelBuilder.Entity<SectionTag>(entity =>
            {
                // Composite primary key from SectionId and TagId
                entity.HasKey(st => new { st.SectionId, st.TagId });

                // Configure relationships
                entity.HasOne(st => st.Section)
                    .WithMany(s => s.SectionTags)
                    .HasForeignKey(st => st.SectionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(st => st.Tag)
                    .WithMany(t => t.SectionTags)
                    .HasForeignKey(st => st.TagId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Index for performance (used for tag lookups)
                entity.HasIndex(st => st.TagId)
                    .HasDatabaseName(DatabaseConstants.SectionTagsTagIdIndex);
            });
        }
    }
}
