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
        private readonly ILogger<SectionController> _logger;

        public SectionController(ISectionService sectionService, ILogger<SectionController> logger)
        {
            _sectionService = sectionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<SectionResponse>>> GetSections()
        {
            var sections = await _sectionService.GetSectionsAsync();
            if (sections == null || !sections.Any())
            {
                _logger.LogInformation("No sections found");
                return Ok(new List<SectionResponse>());
            }
            return Ok(sections);
        }

        [HttpGet("{sectionId:guid}/articles")]
        public async Task<ActionResult<List<ArticleResponse>>> GetArticles(Guid sectionId)
        {
            var articles = await _sectionService.GetArticlesBySectionAsync(sectionId);
            if (articles == null || !articles.Any())
            {
                _logger.LogInformation("No articles found for section {SectionId}", sectionId);
                return Ok(new List<ArticleResponse>());
            }
            return Ok(articles);
        }
    }
}
