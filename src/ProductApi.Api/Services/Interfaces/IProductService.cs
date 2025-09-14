using ProductApi.Api.Models;

namespace ProductApi.Api.Services.Interfaces;

public interface IProductService
{
    Task<Product?> GetProductByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category);
    Task<IEnumerable<Product>> GetProductsWithStockAsync();
    Task<IEnumerable<Product>> GetProductsWithLowStockAsync(int threshold = 10);
    Task<Product> CreateProductAsync(Product product);
    Task<Product?> UpdateProductAsync(int id, Product product);
    Task<bool> DeleteProductAsync(int id);
    Task<bool> DecrementStockAsync(int id, int quantity);
    Task<bool> AddToStockAsync(int id, int quantity);
    Task<bool> ProductExistsAsync(int id);
}

