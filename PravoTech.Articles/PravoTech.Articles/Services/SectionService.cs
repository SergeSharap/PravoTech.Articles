using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PravoTech.Articles.Constants;
using PravoTech.Articles.Data;
using PravoTech.Articles.DTOs;
using PravoTech.Articles.Entities;

namespace PravoTech.Articles.Services
{
    public class SectionService : ISectionService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SectionService> _logger;
        private readonly IMemoryCache _cache;

        public SectionService(AppDbContext dbContext, ILogger<SectionService> logger, IMemoryCache cache)
        {
            _context = dbContext;
            _logger = logger;
            _cache = cache;
        }

        public async Task<PaginatedResponse<SectionResponse>> GetSectionsAsync(int page = 1, int pageSize = 20)
        {
            // Validate pagination parameters
            page = Math.Max(BusinessConstants.MinPageNumber, page);
            pageSize = Math.Max(BusinessConstants.MinPageSize, Math.Min(BusinessConstants.MaxPageSize, pageSize));

            const string cacheKey = "sections_list";
            
            if (_cache.TryGetValue(cacheKey, out List<SectionResponse>? cachedSections))
            {
                // Apply pagination to cached results
                var cachedTotalCount = cachedSections?.Count ?? 0;
                var cachedTotalPages = (int)Math.Ceiling((double)cachedTotalCount / pageSize);
                var skip = (page - 1) * pageSize;
                var items = cachedSections?.Skip(skip).Take(pageSize).ToList() ?? new List<SectionResponse>();

                return new PaginatedResponse<SectionResponse>
                {
                    Items = items,
                    Metadata = new PaginationMetadata
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = cachedTotalCount,
                        TotalPages = cachedTotalPages,
                        HasPreviousPage = page > 1,
                        HasNextPage = page < cachedTotalPages
                    }
                };
            }

            // Get total count for pagination metadata
            var totalSectionsCount = await _context.Sections.CountAsync();

            // Optimized query with EF Core pagination
            var sections = await _context.Sections
                .Include(s => s.SectionTags)
                    .ThenInclude(st => st.Tag)
                .Select(s => new SectionResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                    Tags = s.SectionTags
                        .OrderBy(st => st.Tag.Name)
                        .Select(st => st.Tag.Name)
                        .ToList(),
                    ArticlesCount = s.SectionTags
                        .SelectMany(st => st.Tag.ArticleTags)
                        .Select(at => at.ArticleId)
                        .Distinct()
                        .Count()
                })
                .OrderByDescending(s => s.ArticlesCount)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Cache result for 5 minutes
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

            _cache.Set(cacheKey, sections, cacheOptions);

            var totalPages = (int)Math.Ceiling((double)totalSectionsCount / pageSize);

            _logger.LogInformation("Retrieved {Count} sections from database (page {Page} of {TotalPages})", 
                sections.Count, page, totalPages);

            return new PaginatedResponse<SectionResponse>
            {
                Items = sections,
                Metadata = new PaginationMetadata
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalSectionsCount,
                    TotalPages = totalPages,
                    HasPreviousPage = page > 1,
                    HasNextPage = page < totalPages
                }
            };
        }

        public async Task<PaginatedResponse<ArticleResponse>> GetArticlesBySectionAsync(Guid sectionId, int page = 1, int pageSize = 20)
        {
            // Validate pagination parameters
            page = Math.Max(BusinessConstants.MinPageNumber, page);
            pageSize = Math.Max(BusinessConstants.MinPageSize, Math.Min(BusinessConstants.MaxPageSize, pageSize));

            var section = await _context.Sections
                .Include(s => s.SectionTags)
                    .ThenInclude(st => st.Tag)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

            if (section == null)
            {
                _logger.LogWarning("Section with ID {SectionId} not found", sectionId);
                throw new KeyNotFoundException($"Section with ID {sectionId} not found");
            }

            // Get section tags
            var sectionTagIds = section.SectionTags
                .Select(st => st.TagId)
                .OrderBy(id => id)
                .ToList();

            // Optimized query for getting articles with pagination
            var articlesQuery = _context.Articles
                .Include(a => a.ArticleTags)
                    .ThenInclude(at => at.Tag)
                .AsNoTracking(); // Improve performance for read-only operations

            IQueryable<ArticleResponse> articlesResponseQuery;

            if (!sectionTagIds.Any())
            {
                // Articles without tags
                articlesResponseQuery = articlesQuery
                    .Where(a => !a.ArticleTags.Any())
                    .OrderByDescending(a => a.EffectiveDate)
                    .Select(a => new ArticleResponse
                    {
                        Id = a.Id,
                        Title = a.Title,
                        CreatedAt = a.CreatedAt,
                        UpdatedAt = a.UpdatedAt,
                        RowVersion = a.RowVersion,
                        Tags = new List<string>()
                    });
            }
            else
            {
                // Articles with specific tags
                articlesResponseQuery = articlesQuery
                    .Where(a => a.ArticleTags.Count == sectionTagIds.Count && 
                               a.ArticleTags.All(at => sectionTagIds.Contains(at.TagId)))
                    .OrderByDescending(a => a.EffectiveDate)
                    .Select(a => new ArticleResponse
                    {
                        Id = a.Id,
                        Title = a.Title,
                        CreatedAt = a.CreatedAt,
                        UpdatedAt = a.UpdatedAt,
                        RowVersion = a.RowVersion,
                        Tags = a.ArticleTags
                            .OrderBy(at => at.Order)
                            .Select(at => at.Tag.Name)
                            .ToList()
                    });
            }

            // Get total count for pagination metadata
            var totalCount = await articlesResponseQuery.CountAsync();

            // Apply pagination
            var articles = await articlesResponseQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            _logger.LogInformation("Retrieved {Count} articles with tags {TagIds} for section {SectionId} (page {Page} of {TotalPages})", 
                articles.Count, string.Join(",", sectionTagIds), sectionId, page, totalPages);

            return new PaginatedResponse<ArticleResponse>
            {
                Items = articles,
                Metadata = new PaginationMetadata
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasPreviousPage = page > 1,
                    HasNextPage = page < totalPages
                }
            };
        }

        public async Task<Guid?> GetSectionIdByTags(string? tagKey)
        {
            if (string.IsNullOrEmpty(tagKey))
            {
                // Section without tags
                var section = await _context.Sections
                    .Where(s => !s.SectionTags.Any())
                    .Select(s => s.Id)
                    .FirstOrDefaultAsync();

                return section;
            }
            else
            {
                // Parse tagKey
                var tagIds = tagKey.Split(TagConstants.TagIdSeparator, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .OrderBy(id => id)
                    .ToList();

                if (!tagIds.Any())
                    return null;

                // Find section with exact tag match
                var section = await _context.Sections
                    .Where(s => s.SectionTags.Count == tagIds.Count && 
                               s.SectionTags.All(st => tagIds.Contains(st.TagId)))
                    .Select(s => s.Id)
                    .FirstOrDefaultAsync();

                return section;
            }
        }

        public async Task CreateSection(List<Tag> allTags, List<int> sortedTagIds)
        {
            var sortedTagNames = allTags
                .Where(t => sortedTagIds.Contains(t.Id))
                .OrderBy(t => t.Name)
                .Select(t => t.Name)
                .ToList();

            var sectionName = sortedTagNames.Any()
                ? string.Join(TagConstants.TagNameSeparator, sortedTagNames)
                    [..Math.Min(TagConstants.MaxSectionNameLength, 
                    string.Join(TagConstants.TagNameSeparator, sortedTagNames).Length)]
                : TagConstants.NoTagsSectionName;

            var sectionToAdd = new Section
            {
                Name = sectionName,
                SectionTags = sortedTagIds.Select(tagId => new SectionTag
                {
                    TagId = tagId
                }).ToList()
            };

            _context.Sections.Add(sectionToAdd);
            await _context.SaveChangesAsync();

            // Invalidate cache
            _cache.Remove("sections_list");

            _logger.LogInformation("Created new section with ID {SectionId} and name {SectionName}", 
                sectionToAdd.Id, sectionToAdd.Name);
        }

        public async Task<bool> DeleteSectionIfNoOtherArticlesAsync(List<int> tagIds)
        {
            string? tagKey = tagIds.Any()
                ? string.Join(TagConstants.TagIdSeparator, tagIds.OrderBy(id => id))
                : null;

            // Check if articles exist with these tags
            bool hasArticles;
            if (!tagIds.Any())
            {
                // Articles without tags
                hasArticles = await _context.Articles
                    .AnyAsync(a => !a.ArticleTags.Any());
            }
            else
            {
                // Articles with specific tags
                hasArticles = await _context.Articles
                    .AnyAsync(a => a.ArticleTags.Count == tagIds.Count && 
                                   a.ArticleTags.All(at => tagIds.Contains(at.TagId)));
            }

            if (hasArticles)
            {
                _logger.LogInformation("Section with tags {TagIds} has articles, skipping deletion", tagIds);
                return false;
            }

            // Delete section
            var sectionId = await GetSectionIdByTags(tagKey);
            if (sectionId is not null)
            {
                var section = await _context.Sections.FindAsync(sectionId.Value);
                if (section is not null)
                {
                    _context.Sections.Remove(section);
                    await _context.SaveChangesAsync();
                    
                    // Invalidate cache
                    _cache.Remove("sections_list");
                    
                    _logger.LogInformation("Deleted section {SectionId} with tags {TagIds}", sectionId, tagIds);
                    return true;
                }
            }

            _logger.LogInformation("No section found for tags {TagIds}", tagIds);
            return false;
        }
    }
}
