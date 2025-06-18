namespace PravoTech.Articles.Entities
{
    public class ArticleTag
    {
        public Guid ArticleId { get; set; }
        public virtual Article Article { get; set; } = null!;

        public int TagId { get; set; }
        public virtual Tag Tag { get; set; } = null!;

        public int Order { get; set; }
    }
}
