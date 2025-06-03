using Microsoft.EntityFrameworkCore;
using PravoTech.Articles.Entities;

namespace PravoTech.Articles.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Article> Articles => Set<Article>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<ArticleTag> ArticleTags => Set<ArticleTag>();
        public DbSet<SectionTag> SectionTags => Set<SectionTag>();
        public DbSet<Section> Sections => Set<Section>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Article>()
                .Property(a => a.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            modelBuilder.Entity<Article>()
                .Property(a => a.EffectiveDate)
                .HasComputedColumnSql("ISNULL([UpdatedAt], [CreatedAt])", stored: true);

            modelBuilder.Entity<Article>()
                .HasIndex(a => a.EffectiveDate)
                .HasDatabaseName("IX_Articles_EffectiveDate");

            modelBuilder.Entity<Section>()
                .Property(s => s.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            modelBuilder.Entity<ArticleTag>()
                .HasKey(at => new { at.ArticleId, at.TagId });

            modelBuilder.Entity<ArticleTag>()
                .HasIndex(at => new { at.ArticleId, at.Order }).IsUnique();

            modelBuilder.Entity<SectionTag>()
                .HasKey(at => new { at.SectionId, at.TagId });


            modelBuilder.Entity<Tag>().HasIndex(t => t.NormalizedName)
                .IsUnique()
                .HasDatabaseName("IX_Tags_NormalizedName");
        }
    }
}
