using PravoTech.Articles.DTOs;

namespace PravoTech.Articles.Services
{
    public interface IArticleService
    {
        Task<ArticleResponse?> GetByIdAsync(Guid id);
        Task<ArticleResponse> CreateAsync(CreateArticleRequest request);
        Task<ArticleResponse> UpdateAsync(Guid id, UpdateArticleRequest request);
        Task<bool> DeleteAsync(Guid id);
    }
}
