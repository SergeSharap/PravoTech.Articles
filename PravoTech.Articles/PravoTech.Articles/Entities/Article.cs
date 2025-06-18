using System.ComponentModel.DataAnnotations;
using PravoTech.Articles.Constants;

namespace PravoTech.Articles.Entities
{
    public class Article
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime EffectiveDate { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        public virtual ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();

        public void SetEffectiveDate()
        {
            EffectiveDate = UpdatedAt ?? CreatedAt;
        }
    }
}
