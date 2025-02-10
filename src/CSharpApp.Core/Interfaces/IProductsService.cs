namespace CSharpApp.Core.Interfaces;

public interface IProductsService
{
    Task<IReadOnlyCollection<Product>> GetProducts();
    Task<string> CreateProduct(Product? product);
    Task<Product> GetProduct(int id);
 
}