using ProductApi.Api.Models;

namespace ProductApi.Api.Repositories.Interfaces;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category);
    Task<IEnumerable<Product>> GetProductsWithStockAsync();
    Task<IEnumerable<Product>> GetProductsWithLowStockAsync(int threshold = 10);
    Task<bool> UpdateStockAsync(int productId, int newStock);
    Task<bool> DecrementStockAsync(int productId, int quantity);
    Task<bool> AddToStockAsync(int productId, int quantity);
    Task<int> GenerateNextProductIdAsync();
}

