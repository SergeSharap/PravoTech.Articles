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

        public List<ArticleTag> ArticleTags { get; set; } = [];
        public List<SectionTag> SectionTags { get; set; } = [];
    }
}
