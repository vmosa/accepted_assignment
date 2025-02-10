namespace CSharpApp.Core.Interfaces;

public interface ICategoriesService
{
    Task<IReadOnlyCollection<Category>> GetCategories();
    Task<Category> CreateCategory(Category? category);
    Task<Category> GetCategory(int id);

}