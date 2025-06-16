using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PravoTech.Articles.Constants;
using PravoTech.Articles.Data;
using PravoTech.Articles.Entities;
using System.ComponentModel.DataAnnotations;

namespace PravoTech.Articles.Services
{
    public class TagService : ITagService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TagService> _logger;
        private readonly IMemoryCache _cache;
        private const string TagCacheKeyPrefix = "Tag_";

        public TagService(
            AppDbContext context, 
            ILogger<TagService> logger,
            IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        public async Task<List<Tag>> GetOrCreateTagsAsync(List<string> tags)
        {
            var normalizedTags = tags
                .Select(t => t.Trim().ToLowerInvariant())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .ToList();

            if (normalizedTags.Count > BusinessConstants.MaxTagsPerArticle)
            {
                _logger.LogWarning("Too many tags requested: {Count}", normalizedTags.Count);
                throw new ValidationException(BusinessConstants.TooManyTagsErrorMessage);
            }

            var existingTags = new List<Tag>();
            var tagsToCreate = new List<string>();

            foreach (var normalizedTag in normalizedTags)
            {
                var cacheKey = $"{TagCacheKeyPrefix}{normalizedTag}";
                if (_cache.TryGetValue(cacheKey, out Tag? cachedTag))
                {
                    existingTags.Add(cachedTag!);
                }
                else
                {
                    tagsToCreate.Add(normalizedTag);
                }
            }

            if (tagsToCreate.Any())
            {
                var newTagsFromDb = await _context.Tags
                    .Where(t => tagsToCreate.Contains(t.NormalizedName))
                    .ToListAsync();

                var existingSet = newTagsFromDb.Select(et => et.NormalizedName).ToHashSet();
                var newTags = tagsToCreate
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
                    _logger.LogInformation("Created {Count} new tags", newTags.Count);
                }

                var allNewTags = newTagsFromDb.Concat(newTags).ToList();
                foreach (var tag in allNewTags)
                {
                    _cache.Set($"{TagCacheKeyPrefix}{tag.NormalizedName}", tag, TimeSpan.FromHours(1));
                }

                existingTags.AddRange(allNewTags);
            }

            _logger.LogInformation("Retrieved {Count} tags", existingTags.Count);
            return existingTags;
        }
    }
}
