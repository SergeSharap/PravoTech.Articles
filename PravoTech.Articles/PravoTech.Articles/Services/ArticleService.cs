using Microsoft.EntityFrameworkCore;
using PravoTech.Articles.Constants;
using PravoTech.Articles.Data;
using PravoTech.Articles.DTOs;
using PravoTech.Articles.Entities;

namespace PravoTech.Articles.Services
{
    public class ArticleService : IArticleService
    {
        private readonly AppDbContext _context;
        private readonly ISectionService _sectionService;
        private readonly ITagService _tagService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<ArticleService> _logger;

        public ArticleService(
            AppDbContext dbContext, 
            ISectionService sectionService, 
            ITagService tagService,
            IDateTimeProvider dateTimeProvider,
            ILogger<ArticleService> logger)
        {
            _context = dbContext;
            _sectionService = sectionService;
            _tagService = tagService;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task<ArticleResponse?> GetByIdAsync(Guid id)
        {
            var article = await _context.Articles
                .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
            {
                _logger.LogWarning("Article with ID {ArticleId} not found", id);
                return null;
            }

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
                Tags = tags,
                RowVersion = article.RowVersion
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
                    _logger.LogInformation("Creating new article with title {Title}", request.Title);

                    Article article;
                    // Handle case with empty tags
                    if (!request.Tags.Any())
                    {
                        article = new Article
                        {
                            Title = request.Title,
                            CreatedAt = _dateTimeProvider.UtcNow,
                            ArticleTags = new List<ArticleTag>() // Empty list
                        };
                        article.SetEffectiveDate();

                        _context.Articles.Add(article);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation("Created article {ArticleId} without tags", article.Id);
                        return new ArticleResponse
                        {
                            Id = article.Id,
                            Title = article.Title,
                            CreatedAt = article.CreatedAt,
                            Tags = new List<string>(),
                            RowVersion = article.RowVersion
                        };
                    }

                    // Handle case with tags
                    List<Tag> allTags = await GetTagsAndCreateSectionIfNeeded(request.Tags);
                    var tagDict = allTags.ToDictionary(t => t.NormalizedName);

                    article = new Article
                    {
                        Title = request.Title,
                        CreatedAt = _dateTimeProvider.UtcNow,
                        ArticleTags = request.Tags
                            .Select((tag, i) => {
                                var normalized = tag.Trim().ToLowerInvariant();
                                if (!tagDict.TryGetValue(normalized, out var tagEntity))
                                    throw new InvalidOperationException(
                                        string.Format(BusinessConstants.TagNotFoundErrorMessageTemplate, normalized));

                                return new ArticleTag
                                {
                                    TagId = tagEntity.Id,
                                    Order = i
                                };
                            })
                            .ToList()
                    };
                    article.SetEffectiveDate();

                    _context.Articles.Add(article);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Created article {ArticleId} with {TagCount} tags", article.Id, request.Tags.Count);
                    return new ArticleResponse
                    {
                        Id = article.Id,
                        Title = article.Title,
                        CreatedAt = article.CreatedAt,
                        Tags = request.Tags,
                        RowVersion = article.RowVersion
                    };
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
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
                    _logger.LogInformation("Updating article {ArticleId}", id);

                    var article = await _context.Articles
                        .Include(a => a.ArticleTags)
                        .FirstOrDefaultAsync(a => a.Id == id);

                    if (article == null)
                    {
                        _logger.LogWarning("Article with ID {ArticleId} not found for update", id);
                        throw new KeyNotFoundException($"Article with ID {id} not found");
                    }

                    // Check for concurrency conflict
                    if (!article.RowVersion.SequenceEqual(request.RowVersion))
                    {
                        throw new InvalidOperationException("The article has been modified by another user. Please refresh and try again.");
                    }

                    var oldTagIds = article.ArticleTags
                        .Select(at => at.TagId)
                        .OrderBy(id => id)
                        .ToList();

                    article.Title = request.Title;
                    article.UpdatedAt = _dateTimeProvider.UtcNow;
                    article.SetEffectiveDate();

                    // Remove old tag relationships
                    _context.ArticleTags.RemoveRange(article.ArticleTags);

                    // Handle case with empty tags
                    if (!request.Tags.Any())
                    {
                        article.ArticleTags = new List<ArticleTag>();
                    }
                    else
                    {
                        // Handle case with tags
                        List<Tag> allTags = await GetTagsAndCreateSectionIfNeeded(request.Tags);
                        var tagDict = allTags.ToDictionary(t => t.NormalizedName);
                        
                        article.ArticleTags = request.Tags
                            .Select((tag, i) =>
                            {
                                var normalized = tag.Trim().ToLowerInvariant();
                                if (!tagDict.TryGetValue(normalized, out var tagEntity))
                                    throw new InvalidOperationException(
                                        string.Format(BusinessConstants.TagNotFoundErrorMessageTemplate, normalized));

                                return new ArticleTag
                                {
                                    ArticleId = article.Id,
                                    TagId = tagEntity.Id,
                                    Order = i
                                };
                            })
                            .ToList();
                    }

                    await _context.SaveChangesAsync();
                    await _sectionService.DeleteSectionIfNoOtherArticlesAsync(oldTagIds);
                    await transaction.CommitAsync();

                    _logger.LogInformation("Updated article {ArticleId} with {TagCount} tags", id, request.Tags.Count);
                    return new ArticleResponse
                    {
                        Id = article.Id,
                        Title = article.Title,
                        CreatedAt = article.CreatedAt,
                        UpdatedAt = article.UpdatedAt,
                        Tags = request.Tags,
                        RowVersion = article.RowVersion
                    };
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException("The article has been modified by another user. Please refresh and try again.", ex);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
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
                    _logger.LogInformation("Deleting article {ArticleId}", id);

                    var article = await _context.Articles
                        .Include(a => a.ArticleTags)
                        .ThenInclude(at => at.Tag)
                        .FirstOrDefaultAsync(a => a.Id == id);

                    if (article is null)
                    {
                        _logger.LogWarning("Article with ID {ArticleId} not found for deletion", id);
                        return false;
                    }

                    var tagIds = article.ArticleTags
                        .Select(at => at.TagId)
                        .OrderBy(id => id)
                        .ToList();

                    _context.ArticleTags.RemoveRange(article.ArticleTags);
                    _context.Articles.Remove(article);
                    await _context.SaveChangesAsync();

                    await _sectionService.DeleteSectionIfNoOtherArticlesAsync(tagIds);
                    await transaction.CommitAsync();

                    _logger.LogInformation("Deleted article {ArticleId}", id);
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        private async Task<List<Tag>> GetTagsAndCreateSectionIfNeeded(List<string> tags)
        {
            Guid? sectionId;
            // Handle case with empty tags
            if (!tags.Any())
            {
                // Check if section without tags exists
                sectionId = await _sectionService.GetSectionIdByTags(null);
                if (sectionId is null)
                {
                    // Create section without tags
                    await _sectionService.CreateSection(new List<Tag>(), new List<int>());
                }
                return new List<Tag>();
            }

            // Handle case with tags
            var allTags = await _tagService.GetOrCreateTagsAsync(tags);
            var sortedTagIds = allTags
                .Select(t => t.Id)
                .OrderBy(id => id)
                .ToList();
            var tagKey = string.Join(TagConstants.TagIdSeparator, sortedTagIds);

            sectionId = await _sectionService.GetSectionIdByTags(tagKey);
            if (sectionId is null)
            {
                await _sectionService.CreateSection(allTags, sortedTagIds);
            }

            return allTags;
        }
    }
}
