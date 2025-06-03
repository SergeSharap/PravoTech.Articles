namespace PravoTech.Articles.Entities
{
    public class SectionTag
    {
        public Guid SectionId { get; set; }
        public Section Section { get; set; } = null!;

        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
