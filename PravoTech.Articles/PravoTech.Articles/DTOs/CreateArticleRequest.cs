using System.ComponentModel.DataAnnotations;

namespace PravoTech.Articles.DTOs
{
    public class CreateArticleRequest
    {
        [Required(AllowEmptyStrings = false)]
        [MaxLength(256)]
        public string Title { get; set; } = null!;

        [MaxLength(256)]
        public List<string> Tags { get; set; } = [];
    }
}
