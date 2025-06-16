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

        public List<ArticleTag> ArticleTags { get; set; } = new();

        public void SetEffectiveDate()
        {
            EffectiveDate = UpdatedAt ?? CreatedAt;
        }
    }
}
