using PravoTech.Articles.Constants;
using System.ComponentModel.DataAnnotations;

namespace PravoTech.Articles.Entities
{
    public class Section
    {
        public Guid Id { get; set; }

        [MaxLength(ValidationConstants.MaxSectionNameLength)]
        public string Name { get; set; } = null!;

        public List<SectionTag> SectionTags { get; set; } = [];
    }
}
