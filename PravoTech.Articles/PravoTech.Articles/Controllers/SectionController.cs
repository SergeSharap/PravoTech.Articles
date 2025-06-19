using Microsoft.AspNetCore.Mvc;
using PravoTech.Articles.Constants;
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

        /// <summary>
        /// Get all sections with pagination
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 20, max: 100)</param>
        /// <returns>Paginated list of sections</returns>
        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<SectionResponse>>> GetSections(
            int page = BusinessConstants.MinPageNumber,
            int pageSize = BusinessConstants.DefaultPageSize)
        {
            var sections = await _sectionService.GetSectionsAsync(page, pageSize);
            return Ok(sections);
        }

        /// <summary>
        /// Get articles for a specific section with pagination
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 20, max: 100)</param>
        /// <returns>Paginated list of articles for the section</returns>
        [HttpGet("{sectionId:guid}/articles")]
        public async Task<ActionResult<PaginatedResponse<ArticleResponse>>> GetArticles(
            Guid sectionId,
            int page = BusinessConstants.MinPageNumber,
            int pageSize = BusinessConstants.DefaultPageSize)
        {
            var articles = await _sectionService.GetArticlesBySectionAsync(sectionId, page, pageSize);
            return Ok(articles);
        }
    }
}
