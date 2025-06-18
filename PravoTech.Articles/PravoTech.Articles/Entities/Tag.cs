using System.ComponentModel.DataAnnotations;
using PravoTech.Articles.Constants;

namespace PravoTech.Articles.Entities
{
    public class Tag
    {
        public int Id { get; set; }

        [MaxLength(ValidationConstants.MaxTagNameLength)]
        public string Name { get; set; } = null!;

        [MaxLength(ValidationConstants.MaxTagNameLength)]
        public string NormalizedName { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
        public virtual ICollection<SectionTag> SectionTags { get; set; } = new List<SectionTag>();
    }
}
