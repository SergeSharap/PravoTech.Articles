namespace PravoTech.Articles.DTOs
{
    public class ArticleFlat
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? TagName { get; set; } = null!;
        public int? TagOrder { get; set; }
    }
}
