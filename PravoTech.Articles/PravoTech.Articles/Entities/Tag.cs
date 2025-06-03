using System.ComponentModel.DataAnnotations;

namespace PravoTech.Articles.Entities
{
    public class Tag
    {
        public int Id { get; set; }

        [MaxLength(256)]
        public string Name { get; set; } = null!;
        [MaxLength(256)]
        public string NormalizedName { get; set; } = null!;

        public List<ArticleTag> ArticleTags { get; set; } = [];
        public List<SectionTag> SectionTags { get; set; } = [];
    }
}
