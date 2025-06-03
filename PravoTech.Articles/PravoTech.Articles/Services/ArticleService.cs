using Microsoft.EntityFrameworkCore;
using PravoTech.Articles.Data;
using PravoTech.Articles.DTOs;
using PravoTech.Articles.Entities;

namespace PravoTech.Articles.Services
{
    public class ArticleService : IArticleService
    {
        public ArticleService(AppDbContext dbContext, ISectionService sectionService, ITagService tagService)
        {
            _context = dbContext;
            _sectionService = sectionService;
            _tagService = tagService;
        }

        private readonly AppDbContext _context;
        private readonly ISectionService _sectionService;
        private readonly ITagService _tagService;

        public async Task<ArticleResponse?> GetByIdAsync(Guid id)
        {
            var article = await _context.Articles
                .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
                return null;

            var tags = article.ArticleTags
                .OrderBy(at => at.Order)
                .Select(at => at.Tag.Name)
                .ToList();

            return new ArticleResponse
            {
                Id = article.Id,
                Title = article.Title,
                CreatedAt = article.CreatedAt,
                UpdatedAt = article.UpdatedAt,
                Tags = tags
            };
        }

        public async Task<ArticleResponse> CreateAsync(CreateArticleRequest request)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    List<Tag> allTags = await GetTagsAndCreateSectionIfNeeded(request.Tags);
                    var tagDict = allTags.ToDictionary(t => t.NormalizedName);

                    var article = new Article
                    {
                        Title = request.Title,
                        CreatedAt = DateTime.UtcNow,
                        ArticleTags = request.Tags
                            .Select((tag, i) => {
                                var normalized = tag.Trim().ToLowerInvariant();
                                if (!tagDict.TryGetValue(normalized, out var tagEntity))
                                    throw new InvalidOperationException($"Tag '{normalized}' not found in allTags.");

                                return new ArticleTag
                                {
                                    TagId = tagEntity.Id,
                                    Order = i
                                };
                            })
                            .ToList()
                    };

                    _context.Articles.Add(article);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return new ArticleResponse
                    {
                        Id = article.Id,
                        Title = article.Title,
                        CreatedAt = article.CreatedAt,
                        Tags = request.Tags
                    };
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw; //add logging
                }
            });
        }

        public async Task<ArticleResponse> UpdateAsync(Guid id, UpdateArticleRequest request)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var article = await _context.Articles
                        .Include(a => a.ArticleTags)
                        .FirstOrDefaultAsync(a => a.Id == id);

                    if (article == null)
                        throw new InvalidOperationException("Article not found");

                    var oldTagIds = article.ArticleTags
                        .Select(at => at.TagId)
                        .OrderBy(id => id)
                        .ToList();
                    List<Tag> allTags = await GetTagsAndCreateSectionIfNeeded(request.Tags);

                    article.Title = request.Title;
                    article.UpdatedAt = DateTime.UtcNow;

                    _context.ArticleTags.RemoveRange(article.ArticleTags);

                    var tagDict = allTags.ToDictionary(t => t.NormalizedName);
                    article.ArticleTags = request.Tags
                        .Select((tag, i) =>
                        {
                            var normalized = tag.Trim().ToLowerInvariant();
                            if (!tagDict.TryGetValue(normalized, out var tagEntity))
                                throw new InvalidOperationException($"Tag '{normalized}' not found in allTags.");

                            return new ArticleTag
                            {
                                ArticleId = article.Id,
                                TagId = tagEntity.Id,
                                Order = i
                            };
                        })
                        .ToList();

                    await _context.SaveChangesAsync();

                    await _sectionService.DeleteSectionIfNoOtherArticlesAsync(oldTagIds);

                    await transaction.CommitAsync();

                    return new ArticleResponse
                    {
                        Id = article.Id,
                        Title = article.Title,
                        CreatedAt = article.CreatedAt,
                        UpdatedAt = article.UpdatedAt,
                        Tags = request.Tags
                    };
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw; //add logging
                }
            }); 
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var article = await _context.Articles
                        .Include(a => a.ArticleTags)
                        .ThenInclude(at => at.Tag)
                        .FirstOrDefaultAsync(a => a.Id == id);

                    if (article is null)
                        return false;

                    var tagIds = article.ArticleTags
                        .Select(at => at.TagId)
                        .OrderBy(id => id)
                        .ToList();

                    _context.ArticleTags.RemoveRange(article.ArticleTags);
                    _context.Articles.Remove(article);
                    await _context.SaveChangesAsync();

                    await _sectionService.DeleteSectionIfNoOtherArticlesAsync(tagIds);

                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw; //add logging
                }
            });

        }

        private async Task<List<Tag>> GetTagsAndCreateSectionIfNeeded(List<string> tags)
        {
            var allTags = await _tagService.GetOrCreateTagsAsync(tags);
            var sortedTagIds = allTags
                .Select(t => t.Id)
                .OrderBy(id => id)
                .ToList();
            var tagKey = string.Join(",", sortedTagIds);

            var sectionId = await _sectionService.GetSectionIdByTags(tagKey);
            if (sectionId is null)
            {
                await _sectionService.CreateSection(allTags, sortedTagIds);
            }

            return allTags;
        }
    }
}
