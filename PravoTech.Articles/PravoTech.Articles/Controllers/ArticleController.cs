using Microsoft.AspNetCore.Mvc;
using PravoTech.Articles.DTOs;
using PravoTech.Articles.Services;
using Microsoft.Extensions.Logging;

namespace PravoTech.Articles.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly IArticleService _service;
        private readonly ILogger<ArticleController> _logger;

        public ArticleController(IArticleService service, ILogger<ArticleController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ArticleResponse>> Get(Guid id)
        {
            var article = await _service.GetByIdAsync(id);
            if (article == null)
            {
                throw new KeyNotFoundException($"Article with ID {id} not found");
            }
            return Ok(article);
        }

        [HttpPost]
        public async Task<ActionResult<ArticleResponse>> Create([FromBody] CreateArticleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var article = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(Get), new { id = article.Id }, article);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ArticleResponse>> Update(Guid id, [FromBody] UpdateArticleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var article = await _service.UpdateAsync(id, request);
            if (article == null)
            {
                throw new KeyNotFoundException($"Article with ID {id} not found");
            }
            return Ok(article);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
            {
                throw new KeyNotFoundException($"Article with ID {id} not found");
            }
            return NoContent();
        }
    }
}
