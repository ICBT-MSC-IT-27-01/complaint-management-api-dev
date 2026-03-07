using Cd.Cms.Application.Contracts.Services;
using Cd.Cms.Application.DTOs.Categories;
using Cd.Cms.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cd.Cms.Api.Controllers
{
    [ApiController]
    [Route("api/v1/categories")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _svc;
        public CategoriesController(ICategoryService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _svc.GetAllAsync();
            return Ok(ApiResponse<object>.Success("Categories loaded.", result));
        }

        [HttpGet("parents")]
        public async Task<IActionResult> GetParents()
        {
            var result = await _svc.GetParentsAsync();
            return Ok(ApiResponse<object>.Success("Parent categories loaded.", result));
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _svc.GetByIdAsync(id);
            return result == null ? NotFound(ApiResponse<object>.NotFound()) : Ok(ApiResponse<object>.Success("Category loaded.", result));
        }

        [HttpPost("parents")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateParent([FromBody] CreateParentCategoryRequest dto)
        {
            try
            {
                var result = await _svc.CreateParentAsync(dto, GetActorUserId());
                return StatusCode(201, ApiResponse<object>.Success("Parent category created.", result));
            }
            catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.ValidationError(ex.Message)); }
            catch (Exception ex) { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest dto)
        {
            try
            {
                var result = await _svc.CreateAsync(dto, GetActorUserId());
                return StatusCode(201, ApiResponse<object>.Success("Category created.", result));
            }
            catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.ValidationError(ex.Message)); }
            catch (Exception ex)         { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(long id, [FromBody] CreateCategoryRequest dto)
        {
            try { await _svc.UpdateAsync(id, dto, GetActorUserId()); return Ok(ApiResponse<object>.Success("Category updated.")); }
            catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.ValidationError(ex.Message)); }
            catch (Exception ex) { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate(long id)
        {
            try { await _svc.DeactivateAsync(id, GetActorUserId()); return Ok(ApiResponse<object>.Success("Category deactivated.")); }
            catch (Exception ex) { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        private long GetActorUserId() => long.Parse(User.FindFirst("uid")?.Value ?? "0");
    }
}
