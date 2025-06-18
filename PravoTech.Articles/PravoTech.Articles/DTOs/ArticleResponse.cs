namespace PravoTech.Articles.DTOs
{
    public class ArticleResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<string> Tags { get; set; } = new();
        public byte[] RowVersion { get; set; } = null!;
    }
}
