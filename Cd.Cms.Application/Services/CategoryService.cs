using Cd.Cms.Application.Contracts.Repositories;
using Cd.Cms.Application.Contracts.Services;
using Cd.Cms.Application.DTOs.Categories;

namespace Cd.Cms.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;
        public CategoryService(ICategoryRepository repo) => _repo = repo;

        public Task<List<CategoryDto>> GetParentsAsync() => _repo.GetParentsAsync();
        public Task<List<CategoryDto>> GetAllAsync() => _repo.GetAllAsync();
        public Task<CategoryDto?> GetByIdAsync(long id) => _repo.GetByIdAsync(id);
        public Task<CategoryDto> CreateParentAsync(CreateParentCategoryRequest request, long actorUserId)
        {
            if (string.IsNullOrWhiteSpace(request.Name)) throw new ArgumentException("Parent category name is required.");
            return _repo.CreateParentAsync(request, actorUserId);
        }
        public Task<CategoryDto> CreateAsync(CreateCategoryRequest request, long actorUserId)
        {
            if (string.IsNullOrWhiteSpace(request.Name)) throw new ArgumentException("Category name is required.");
            if (!request.ParentCategoryId.HasValue || request.ParentCategoryId.Value <= 0)
                throw new ArgumentException("Parent category is required.");
            return _repo.CreateAsync(request, actorUserId);
        }
        public Task UpdateAsync(long id, CreateCategoryRequest request, long actorUserId)
        {
            if (string.IsNullOrWhiteSpace(request.Name)) throw new ArgumentException("Category name is required.");
            if (!request.ParentCategoryId.HasValue || request.ParentCategoryId.Value <= 0)
                throw new ArgumentException("Parent category is required.");
            return _repo.UpdateAsync(id, request, actorUserId);
        }
        public Task DeactivateAsync(long id, long actorUserId) => _repo.DeactivateAsync(id, actorUserId);
    }
}
