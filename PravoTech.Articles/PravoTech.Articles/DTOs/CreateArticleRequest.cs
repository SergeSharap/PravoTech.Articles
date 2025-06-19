using PravoTech.Articles.Constants;
using System.ComponentModel.DataAnnotations;

namespace PravoTech.Articles.DTOs
{
    public class CreateArticleRequest
    {
        [Required(AllowEmptyStrings = false)]
        [MaxLength(ValidationConstants.MaxTitleLength)]
        public string Title { get; set; } = null!;

        [MaxLength(BusinessConstants.MaxTagsPerArticle)]
        public List<string> Tags { get; set; } = [];
    }
}
