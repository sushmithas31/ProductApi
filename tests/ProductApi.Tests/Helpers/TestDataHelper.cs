using ProductApi.Api.DTOs.Requests;
using ProductApi.Api.Models;

namespace ProductApi.Tests.Helpers;

public static class TestDataHelper
{
    public static Product CreateTestProduct(int id = 100001, string name = "Test Product", decimal price = 99.99m, int stock = 10)
    {
        return new Product
        {
            ProductId = id,
            Name = name,
            Description = "Test product description",
            Price = price,
            StockAvailable = stock,
            Category = "Electronics",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static List<Product> CreateTestProducts(int count = 3)
    {
        var products = new List<Product>();
        for (int i = 0; i < count; i++)
        {
            products.Add(CreateTestProduct(
                id: 100001 + i,
                name: $"Test Product {i + 1}",
                price: 99.99m + i,
                stock: 10 + i
            ));
        }
        return products;
    }

    public static CreateProductRequest CreateTestCreateRequest(string name = "Test Product")
    {
        return new CreateProductRequest
        {
            Name = name,
            Description = "Test product description",
            Price = 99.99m,
            StockAvailable = 10,
            Category = "Electronics"
        };
    }

    public static UpdateProductRequest CreateTestUpdateRequest(string name = "Updated Product")
    {
        return new UpdateProductRequest
        {
            Name = name,
            Description = "Updated product description",
            Price = 149.99m,
            StockAvailable = 15,
            Category = "Electronics"
        };
    }
}

