using Microsoft.EntityFrameworkCore;
using PravoTech.Articles.Constants;
using PravoTech.Articles.Data;
using PravoTech.Articles.DTOs;
using PravoTech.Articles.Entities;

namespace PravoTech.Articles.Services
{
    public class SectionService : ISectionService
    {
        public SectionService(AppDbContext dbContext)
        {
            _context = dbContext;
        }

        private readonly AppDbContext _context;

        public async Task<List<SectionResponse>> GetSectionsAsync()
        {
            try
            {
                var result = await _context.Database
                    .SqlQuery<SectionWithTagsRaw>(SectionSqlQueryBuilder.BuildGetSectionsSqlQuery())
                    .ToListAsync();
                var sections = result.Select(s => new SectionResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                    ArticlesCount = s.ArticlesCount,
                    Tags = s.TagList.Split(SqlQueryConstants.TagIdSeparator, StringSplitOptions.RemoveEmptyEntries).ToList()
                })
                .ToList();

                return sections;
            }
            catch
            {
                throw; //add logging
            }
        }

        public async Task<List<ArticleResponse>> GetArticlesBySectionAsync(Guid sectionId)
        {
            try
            {
                var section = await _context.Sections
                    .Where(st => st.Id == sectionId).ToListAsync();
                if (section.Count == 0)
                {
                    return new List<ArticleResponse>();
                }

                var tagIds = await _context.SectionTags
                .Where(st => st.SectionId == sectionId)
                .Select(st => st.TagId)
                .OrderBy(id => id)
                .ToListAsync();
                var tagKey = string.Join(SqlQueryConstants.TagIdSeparator, tagIds);

                var rawArticles = await _context.Database
                    .SqlQuery<ArticleFlat>(SectionSqlQueryBuilder.BuildGetArticlesBySectionSqlQuery(tagKey))
                    .ToListAsync();

                var grouped = rawArticles
                    .GroupBy(x => new { x.Id, x.Title, x.CreatedAt, x.UpdatedAt })
                    .Select(g => new ArticleResponse
                    {
                        Id = g.Key.Id,
                        Title = g.Key.Title,
                        CreatedAt = g.Key.CreatedAt,
                        UpdatedAt = g.Key.UpdatedAt,
                        Tags = g.OrderBy(x => x.TagOrder).Select(x => x.TagName).ToList()
                    })
                    .ToList();

                return grouped;
            }
            catch
            {
                throw; //add logging
            }
        }

        public async Task<Guid?> GetSectionIdByTags(string tagKey)
        {
            var result = await _context.Database
                .SqlQuery<Guid?>(SectionSqlQueryBuilder.BuildGetSectionIdByTagsSqlQuery(tagKey))
                .ToListAsync();

            return result.FirstOrDefault();
        }

        public async Task CreateSection(List<Tag> allTags, List<int> sortedTagIds)
        {
            var sortedTagNames = allTags
                .Where(t => sortedTagIds.Contains(t.Id))
                .OrderBy(t => t.Name)
                .Select(t => t.Name)
                .ToList();

            var sectionName = sortedTagNames.Any()
                ? string.Join(SqlQueryConstants.TagNameSeparator, sortedTagNames)
                    [..Math.Min(SqlQueryConstants.MaxSectionNameLength, 
                    string.Join(SqlQueryConstants.TagNameSeparator, sortedTagNames).Length)]
                : SqlQueryConstants.NoTagsSectionName;

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
        }

        public async Task<bool> DeleteSectionIfNoOtherArticlesAsync(List<int> tagIds)
        {
            string? tagKey = tagIds.Any()
                ? string.Join(SqlQueryConstants.TagIdSeparator, tagIds.OrderBy(id => id))
                : null;

            var result = await _context.Database.SqlQuery<int>(SectionSqlQueryBuilder.BuildGetArticlesExistenceByTagsSqlQuery(tagKey)).ToListAsync();
            var exists = result.FirstOrDefault();
            if (exists == 1)
                return false;

            var sectionIds = await _context.Database
                .SqlQuery<Guid?>(SectionSqlQueryBuilder.BuildGetSectionIdByTagsSqlQuery(tagKey))
                .ToListAsync();
            var sectionId = sectionIds.FirstOrDefault();

            if (sectionId is not null)
            {
                var section = await _context.Sections.FindAsync(sectionId.Value);
                if (section is not null)
                {
                    _context.Sections.Remove(section);
                    await _context.SaveChangesAsync();
                    return true;
                }
            }

            return false;
        }
    }
}
