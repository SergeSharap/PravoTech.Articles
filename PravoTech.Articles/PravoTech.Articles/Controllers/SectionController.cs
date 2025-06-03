using Microsoft.AspNetCore.Mvc;
using PravoTech.Articles.DTOs;
using PravoTech.Articles.Services;

namespace PravoTech.Articles.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SectionController : ControllerBase
    {
        private readonly ISectionService _sectionService;

        public SectionController(ISectionService sectionService)
        {
            _sectionService = sectionService;
        }

        [HttpGet]
        public async Task<ActionResult<List<SectionResponse>>> GetSections()
        {
            var sections = await _sectionService.GetSectionsAsync();
            return Ok(sections);
        }

        [HttpGet("{sectionId:guid}/articles")]
        public async Task<ActionResult<List<ArticleResponse>>> GetArticles(Guid sectionId)
        {
            var articles = await _sectionService.GetArticlesBySectionAsync(sectionId);
            return Ok(articles);
        }
    }
}
