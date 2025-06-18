namespace PravoTech.Articles.Entities
{
    public class SectionTag
    {
        public Guid SectionId { get; set; }
        public virtual Section Section { get; set; } = null!;

        public int TagId { get; set; }
        public virtual Tag Tag { get; set; } = null!;
    }
}
