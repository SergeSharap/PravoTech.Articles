using System.ComponentModel.DataAnnotations;
using PravoTech.Articles.Constants;

namespace PravoTech.Articles.DTOs
{
    public class UpdateArticleRequest
    {
        [Required(AllowEmptyStrings = false)]
        [MaxLength(ValidationConstants.MaxTitleLength)]
        public string Title { get; set; } = null!;

        [MaxLength(BusinessConstants.MaxTagsPerArticle)]
        public List<string> Tags { get; set; } = [];
    }
}
