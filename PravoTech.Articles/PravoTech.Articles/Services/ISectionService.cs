using PravoTech.Articles.DTOs;
using PravoTech.Articles.Entities;

namespace PravoTech.Articles.Services
{
    public interface ISectionService
    {
        Task<PaginatedResponse<SectionResponse>> GetSectionsAsync(int page = 1, int pageSize = 20);
        Task<PaginatedResponse<ArticleResponse>> GetArticlesBySectionAsync(Guid sectionId, int page = 1, int pageSize = 20);
        Task<Guid?> GetSectionIdByTags(string? tagKey);
        Task CreateSection(List<Tag> allTags, List<int> sortedTagIds);
        Task<bool> DeleteSectionIfNoOtherArticlesAsync(List<int> tagIds);
    }
}
