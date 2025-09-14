using ProductApi.Api.DTOs.Requests;
using ProductApi.Api.DTOs.Responses;
using ProductApi.Api.Models;

namespace ProductApi.Api.Extensions;

public static class ProductMappingExtensions
{
    public static ProductResponse ToResponse(this Product product)
    {
        return new ProductResponse
        {
            ProductId = product.ProductId,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockAvailable = product.StockAvailable,
            Category = product.Category,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    public static Product ToEntity(this CreateProductRequest request)
    {
        return new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockAvailable = request.StockAvailable,
            Category = request.Category
        };
    }

    public static Product ToEntity(this UpdateProductRequest request)
    {
        return new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockAvailable = request.StockAvailable,
            Category = request.Category
        };
    }

    public static IEnumerable<ProductResponse> ToResponse(this IEnumerable<Product> products)
    {
        return products.Select(p => p.ToResponse());
    }
}

