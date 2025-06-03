namespace PravoTech.Articles.DTOs
{
    public class ArticleResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<string> Tags { get; set; } = [];
    }
}
