using Microsoft.AspNetCore.Mvc;
using PravoTech.Articles.DTOs;
using PravoTech.Articles.Services;

namespace PravoTech.Articles.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly IArticleService _service;

        public ArticleController(IArticleService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ArticleResponse>> Get(Guid id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }

        [HttpPost]
        public async Task<ActionResult<ArticleResponse>> Create(CreateArticleRequest request)
        {
            return Ok(await _service.CreateAsync(request));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ArticleResponse>> Update(Guid id, UpdateArticleRequest request)
        {
            return Ok(await _service.UpdateAsync(id, request));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
