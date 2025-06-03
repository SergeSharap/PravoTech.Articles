using PravoTech.Articles.Entities;

namespace PravoTech.Articles.Services
{
    public interface ITagService
    {
        Task<List<Tag>> GetOrCreateTagsAsync(List<string> tags);
    }
}
