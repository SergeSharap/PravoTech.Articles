using PravoTech.Articles.DTOs;
using PravoTech.Articles.Entities;

namespace PravoTech.Articles.Services
{
    public interface ISectionService
    {
        Task<List<SectionResponse>> GetSectionsAsync();
        Task<List<ArticleResponse>> GetArticlesBySectionAsync(Guid sectionId);
        Task<Guid?> GetSectionIdByTags(string tagKey);
        Task CreateSection(List<Tag> allTags, List<int> sortedTagIds);
        Task<bool> DeleteSectionIfNoOtherArticlesAsync(List<int> tagIds);
    }
}
