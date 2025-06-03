using System.ComponentModel.DataAnnotations;

namespace PravoTech.Articles.Entities
{
    public class Section
    {
        public Guid Id { get; set; }

        [MaxLength(1024)]
        public string Name { get; set; } = null!;

        public List<SectionTag> SectionTags { get; set; } = [];
    }
}
