using ProductApi.Api.Models;
using ProductApi.Api.Repositories.Interfaces;
using ProductApi.Api.Services.Interfaces;

namespace ProductApi.Api.Services.Implementations;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Getting product with ID: {ProductId}", id);
            return await _productRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product with ID: {ProductId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        try
        {
            _logger.LogInformation("Getting all products");
            return await _productRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all products");
            throw;
        }
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
    {
        try
        {
            _logger.LogInformation("Getting products by category: {Category}", category);
            return await _productRepository.GetProductsByCategoryAsync(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products by category: {Category}", category);
            throw;
        }
    }

    public async Task<IEnumerable<Product>> GetProductsWithStockAsync()
    {
        try
        {
            _logger.LogInformation("Getting products with stock");
            return await _productRepository.GetProductsWithStockAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products with stock");
            throw;
        }
    }

    public async Task<IEnumerable<Product>> GetProductsWithLowStockAsync(int threshold = 10)
    {
        try
        {
            _logger.LogInformation("Getting products with low stock (threshold: {Threshold})", threshold);
            return await _productRepository.GetProductsWithLowStockAsync(threshold);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products with low stock");
            throw;
        }
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        try
        {
            _logger.LogInformation("Creating new product: {ProductName}", product.Name);
            
            if (string.IsNullOrWhiteSpace(product.Name))
                throw new ArgumentException("Product name is required");
            
            if (product.Price <= 0)
                throw new ArgumentException("Product price must be greater than zero");
            
            if (product.StockAvailable < 0)
                throw new ArgumentException("Stock available cannot be negative");

            var createdProduct = await _productRepository.AddAsync(product);
            _logger.LogInformation("Product created successfully with ID: {ProductId}", createdProduct.ProductId);
            
            return createdProduct;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product: {ProductName}", product.Name);
            throw;
        }
    }

    public async Task<Product?> UpdateProductAsync(int id, Product product)
    {
        try
        {
            _logger.LogInformation("Updating product with ID: {ProductId}", id);
            
            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found for update", id);
                return null;
            }

            if (string.IsNullOrWhiteSpace(product.Name))
                throw new ArgumentException("Product name is required");
            
            if (product.Price <= 0)
                throw new ArgumentException("Product price must be greater than zero");
            
            if (product.StockAvailable < 0)
                throw new ArgumentException("Stock available cannot be negative");

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.StockAvailable = product.StockAvailable;
            existingProduct.Category = product.Category;

            await _productRepository.UpdateAsync(existingProduct);
            _logger.LogInformation("Product updated successfully with ID: {ProductId}", id);
            
            return existingProduct;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID: {ProductId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting product with ID: {ProductId}", id);
            
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found for deletion", id);
                return false;
            }

            await _productRepository.DeleteAsync(product);
            _logger.LogInformation("Product deleted successfully with ID: {ProductId}", id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product with ID: {ProductId}", id);
            throw;
        }
    }

    public async Task<bool> DecrementStockAsync(int id, int quantity)
    {
        try
        {
            _logger.LogInformation("Decrementing stock for product ID: {ProductId}, quantity: {Quantity}", id, quantity);
            
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero");

            var result = await _productRepository.DecrementStockAsync(id, quantity);
            
            if (result)
                _logger.LogInformation("Stock decremented successfully for product ID: {ProductId}", id);
            else
                _logger.LogWarning("Failed to decrement stock for product ID: {ProductId} - insufficient stock or product not found", id);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrementing stock for product ID: {ProductId}", id);
            throw;
        }
    }

    public async Task<bool> AddToStockAsync(int id, int quantity)
    {
        try
        {
            _logger.LogInformation("Adding to stock for product ID: {ProductId}, quantity: {Quantity}", id, quantity);
            
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero");

            var result = await _productRepository.AddToStockAsync(id, quantity);
            
            if (result)
                _logger.LogInformation("Stock added successfully for product ID: {ProductId}", id);
            else
                _logger.LogWarning("Failed to add stock for product ID: {ProductId} - product not found", id);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding stock for product ID: {ProductId}", id);
            throw;
        }
    }

    public async Task<bool> ProductExistsAsync(int id)
    {
        try
        {
            return await _productRepository.ExistsAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if product exists with ID: {ProductId}", id);
            throw;
        }
    }
}

