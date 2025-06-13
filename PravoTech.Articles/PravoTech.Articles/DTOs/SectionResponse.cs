using PravoTech.Articles.Constants;
using System.ComponentModel.DataAnnotations;

namespace PravoTech.Articles.DTOs
{
    public class SectionResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        [MaxLength(BusinessConstants.MaxTagsPerArticle)]
        public List<string> Tags { get; set; } = [];

        public int ArticlesCount { get; set; }
    }
}
