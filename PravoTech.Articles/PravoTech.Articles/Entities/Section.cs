using PravoTech.Articles.Constants;
using System.ComponentModel.DataAnnotations;

namespace PravoTech.Articles.Entities
{
    public class Section
    {
        public Guid Id { get; set; }

        [MaxLength(ValidationConstants.MaxSectionNameLength)]
        public string Name { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<SectionTag> SectionTags { get; set; } = new List<SectionTag>();
    }
}
