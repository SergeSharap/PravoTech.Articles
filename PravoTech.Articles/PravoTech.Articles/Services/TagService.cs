using Microsoft.EntityFrameworkCore;
using PravoTech.Articles.Constants;
using PravoTech.Articles.Data;
using PravoTech.Articles.Entities;
using System.ComponentModel.DataAnnotations;

namespace PravoTech.Articles.Services
{
    public class TagService : ITagService
    {
        private readonly AppDbContext _context;

        public TagService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Tag>> GetOrCreateTagsAsync(List<string> tags)
        {
            var normalizedTags = tags
                .Select(t => t.Trim().ToLowerInvariant())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .ToList();

            if (normalizedTags.Count > BusinessConstants.MaxTagsPerArticle)
                throw new ValidationException(BusinessConstants.TooManyTagsErrorMessage);

            var existingTags = await _context.Tags
                .Where(t => normalizedTags.Contains(t.NormalizedName))
                .ToListAsync();
            var existingSet = existingTags.Select(et => et.NormalizedName).ToHashSet();

            var newTags = normalizedTags
                .Where(nt => !existingSet.Contains(nt))
                .Select(nt => new Tag
                {
                    Name = tags.First(t => string.Equals(t, nt, StringComparison.OrdinalIgnoreCase)),
                    NormalizedName = nt
                })
                .ToList();

            if (newTags.Any())
            {
                _context.Tags.AddRange(newTags);
                await _context.SaveChangesAsync();
            }

            return existingTags.Concat(newTags).ToList();
        }
    }
}
