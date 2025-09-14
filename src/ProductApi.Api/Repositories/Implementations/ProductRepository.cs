using Microsoft.EntityFrameworkCore;
using ProductApi.Api.Data;
using ProductApi.Api.Models;
using ProductApi.Api.Repositories.Interfaces;
using ProductApi.Api.Services.Interfaces;

namespace ProductApi.Api.Repositories.Implementations;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    private readonly IProductIdGeneratorService _idGenerator;

    public ProductRepository(ProductApiDbContext context, IProductIdGeneratorService idGenerator) : base(context)
    {
        _idGenerator = idGenerator;
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
    {
        return await _dbSet
            .Where(p => p.Category.ToLower() == category.ToLower())
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetProductsWithStockAsync()
    {
        return await _dbSet
            .Where(p => p.StockAvailable > 0)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetProductsWithLowStockAsync(int threshold = 10)
    {
        return await _dbSet
            .Where(p => p.StockAvailable <= threshold && p.StockAvailable > 0)
            .OrderBy(p => p.StockAvailable)
            .ToListAsync();
    }

    public async Task<bool> UpdateStockAsync(int productId, int newStock)
    {
        var product = await _dbSet.FindAsync(productId);
        if (product == null) return false;

        product.StockAvailable = newStock;
        product.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DecrementStockAsync(int productId, int quantity)
    {
        var product = await _dbSet.FindAsync(productId);
        if (product == null || product.StockAvailable < quantity) return false;

        product.StockAvailable -= quantity;
        product.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddToStockAsync(int productId, int quantity)
    {
        var product = await _dbSet.FindAsync(productId);
        if (product == null) return false;

        product.StockAvailable += quantity;
        product.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GenerateNextProductIdAsync()
    {
        return await _idGenerator.GenerateNextProductIdAsync();
    }

    public override async Task<Product> AddAsync(Product entity)
    {
        if (entity.ProductId == 0)
        {
            entity.ProductId = await GenerateNextProductIdAsync();
        }

        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        return await base.AddAsync(entity);
    }

    public override async Task UpdateAsync(Product entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await base.UpdateAsync(entity);
    }
}

