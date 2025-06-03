namespace PravoTech.Articles.DTOs
{
    public class SectionWithTagsRaw
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int ArticlesCount { get; set; }
        public string TagList { get; set; } = null!;
    }
}
