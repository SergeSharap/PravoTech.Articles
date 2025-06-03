using System.ComponentModel.DataAnnotations;

namespace PravoTech.Articles.Entities
{
    public class Article
    {
        public Guid Id { get; set; }

        [MaxLength(256)]
        public string Title { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime EffectiveDate { get; private set; }

        public List<ArticleTag> ArticleTags { get; set; } = [];
    }
}
