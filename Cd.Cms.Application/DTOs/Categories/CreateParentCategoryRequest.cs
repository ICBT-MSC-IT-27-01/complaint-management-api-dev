namespace Cd.Cms.Application.DTOs.Categories
{
    public class CreateParentCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public int SortOrder { get; set; } = 0;
    }
}
