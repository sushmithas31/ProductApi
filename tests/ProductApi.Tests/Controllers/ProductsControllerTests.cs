using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProductApi.Api.Controllers;
using ProductApi.Api.DTOs.Requests;
using ProductApi.Api.DTOs.Responses;
using ProductApi.Api.Models;
using ProductApi.Api.Services.Interfaces;
using ProductApi.Tests.Helpers;
using Xunit;

namespace ProductApi.Tests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _mockService;
    private readonly Mock<ILogger<ProductsController>> _mockLogger;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _mockService = new Mock<IProductService>();
        _mockLogger = new Mock<ILogger<ProductsController>>();
        _controller = new ProductsController(_mockService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllProducts_ReturnsOkWithProducts()
    {
        // Arrange
        var products = TestDataHelper.CreateTestProducts(3);
        _mockService.Setup(s => s.GetAllProductsAsync())
                   .ReturnsAsync(products);

        // Act
        var result = await _controller.GetAllProducts();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductResponse>>(okResult.Value);
        Assert.Equal(3, returnedProducts.Count());
        _mockService.Verify(s => s.GetAllProductsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetProductById_ExistingProduct_ReturnsOkWithProduct()
    {
        // Arrange
        var productId = 100001;
        var product = TestDataHelper.CreateTestProduct(productId);
        _mockService.Setup(s => s.GetProductByIdAsync(productId))
                   .ReturnsAsync(product);

        // Act
        var result = await _controller.GetProductById(productId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProduct = Assert.IsType<ProductResponse>(okResult.Value);
        Assert.Equal(productId, returnedProduct.ProductId);
        _mockService.Verify(s => s.GetProductByIdAsync(productId), Times.Once);
    }

    [Fact]
    public async Task GetProductById_NonExistingProduct_ReturnsNotFound()
    {
        // Arrange
        var productId = 999999;
        _mockService.Setup(s => s.GetProductByIdAsync(productId))
                   .ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.GetProductById(productId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Contains(productId.ToString(), notFoundResult.Value?.ToString());
        _mockService.Verify(s => s.GetProductByIdAsync(productId), Times.Once);
    }

    [Fact]
    public async Task CreateProduct_ValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = TestDataHelper.CreateTestCreateRequest();
        var createdProduct = TestDataHelper.CreateTestProduct(100001);
        _mockService.Setup(s => s.CreateProductAsync(It.IsAny<Product>()))
                   .ReturnsAsync(createdProduct);

        // Act
        var result = await _controller.CreateProduct(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedProduct = Assert.IsType<ProductResponse>(createdResult.Value);
        Assert.Equal(createdProduct.ProductId, returnedProduct.ProductId);
        Assert.Equal(nameof(ProductsController.GetProductById), createdResult.ActionName);
        _mockService.Verify(s => s.CreateProductAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task CreateProduct_ServiceThrowsArgumentException_ReturnsBadRequest()
    {
        // Arrange
        var request = TestDataHelper.CreateTestCreateRequest();
        _mockService.Setup(s => s.CreateProductAsync(It.IsAny<Product>()))
                   .ThrowsAsync(new ArgumentException("Invalid product data"));

        // Act
        var result = await _controller.CreateProduct(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid product data", badRequestResult.Value);
        _mockService.Verify(s => s.CreateProductAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProduct_ExistingProduct_ReturnsOkWithUpdatedProduct()
    {
        // Arrange
        var productId = 100001;
        var request = TestDataHelper.CreateTestUpdateRequest();
        var updatedProduct = TestDataHelper.CreateTestProduct(productId);
        updatedProduct.Name = request.Name;
        
        _mockService.Setup(s => s.UpdateProductAsync(productId, It.IsAny<Product>()))
                   .ReturnsAsync(updatedProduct);

        // Act
        var result = await _controller.UpdateProduct(productId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProduct = Assert.IsType<ProductResponse>(okResult.Value);
        Assert.Equal(request.Name, returnedProduct.Name);
        _mockService.Verify(s => s.UpdateProductAsync(productId, It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProduct_NonExistingProduct_ReturnsNotFound()
    {
        // Arrange
        var productId = 999999;
        var request = TestDataHelper.CreateTestUpdateRequest();
        _mockService.Setup(s => s.UpdateProductAsync(productId, It.IsAny<Product>()))
                   .ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.UpdateProduct(productId, request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Contains(productId.ToString(), notFoundResult.Value?.ToString());
        _mockService.Verify(s => s.UpdateProductAsync(productId, It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task DeleteProduct_ExistingProduct_ReturnsNoContent()
    {
        // Arrange
        var productId = 100001;
        _mockService.Setup(s => s.DeleteProductAsync(productId))
                   .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteProduct(productId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.DeleteProductAsync(productId), Times.Once);
    }

    [Fact]
    public async Task DeleteProduct_NonExistingProduct_ReturnsNotFound()
    {
        // Arrange
        var productId = 999999;
        _mockService.Setup(s => s.DeleteProductAsync(productId))
                   .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteProduct(productId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(productId.ToString(), notFoundResult.Value?.ToString());
        _mockService.Verify(s => s.DeleteProductAsync(productId), Times.Once);
    }

    [Fact]
    public async Task DecrementStock_ValidRequest_ReturnsOkWithUpdatedProduct()
    {
        // Arrange
        var productId = 100001;
        var quantity = 5;
        var updatedProduct = TestDataHelper.CreateTestProduct(productId);
        updatedProduct.StockAvailable = 5; // After decrementing 5 from 10
        
        _mockService.Setup(s => s.DecrementStockAsync(productId, quantity))
                   .ReturnsAsync(true);
        _mockService.Setup(s => s.GetProductByIdAsync(productId))
                   .ReturnsAsync(updatedProduct);

        // Act
        var result = await _controller.DecrementStock(productId, quantity);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProduct = Assert.IsType<ProductResponse>(okResult.Value);
        Assert.Equal(5, returnedProduct.StockAvailable);
        _mockService.Verify(s => s.DecrementStockAsync(productId, quantity), Times.Once);
        _mockService.Verify(s => s.GetProductByIdAsync(productId), Times.Once);
    }

    [Fact]
    public async Task DecrementStock_InvalidQuantity_ReturnsBadRequest()
    {
        // Arrange
        var productId = 100001;
        var invalidQuantity = 0;

        // Act
        var result = await _controller.DecrementStock(productId, invalidQuantity);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("greater than zero", badRequestResult.Value?.ToString());
        _mockService.Verify(s => s.DecrementStockAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DecrementStock_ServiceReturnsFalse_ReturnsBadRequest()
    {
        // Arrange
        var productId = 100001;
        var quantity = 15; // More than available stock
        
        _mockService.Setup(s => s.DecrementStockAsync(productId, quantity))
                   .ReturnsAsync(false);

        // Act
        var result = await _controller.DecrementStock(productId, quantity);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("Unable to decrement stock", badRequestResult.Value?.ToString());
        _mockService.Verify(s => s.DecrementStockAsync(productId, quantity), Times.Once);
    }

    [Fact]
    public async Task AddToStock_ValidRequest_ReturnsOkWithUpdatedProduct()
    {
        // Arrange
        var productId = 100001;
        var quantity = 10;
        var updatedProduct = TestDataHelper.CreateTestProduct(productId);
        updatedProduct.StockAvailable = 20; // After adding 10 to 10
        
        _mockService.Setup(s => s.AddToStockAsync(productId, quantity))
                   .ReturnsAsync(true);
        _mockService.Setup(s => s.GetProductByIdAsync(productId))
                   .ReturnsAsync(updatedProduct);

        // Act
        var result = await _controller.AddToStock(productId, quantity);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProduct = Assert.IsType<ProductResponse>(okResult.Value);
        Assert.Equal(20, returnedProduct.StockAvailable);
        _mockService.Verify(s => s.AddToStockAsync(productId, quantity), Times.Once);
        _mockService.Verify(s => s.GetProductByIdAsync(productId), Times.Once);
    }

    [Fact]
    public async Task AddToStock_InvalidQuantity_ReturnsBadRequest()
    {
        // Arrange
        var productId = 100001;
        var invalidQuantity = -5;

        // Act
        var result = await _controller.AddToStock(productId, invalidQuantity);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("greater than zero", badRequestResult.Value?.ToString());
        _mockService.Verify(s => s.AddToStockAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetProductsByCategory_ValidCategory_ReturnsOkWithProducts()
    {
        // Arrange
        var category = "Electronics";
        var products = TestDataHelper.CreateTestProducts(2);
        _mockService.Setup(s => s.GetProductsByCategoryAsync(category))
                   .ReturnsAsync(products);

        // Act
        var result = await _controller.GetProductsByCategory(category);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductResponse>>(okResult.Value);
        Assert.Equal(2, returnedProducts.Count());
        _mockService.Verify(s => s.GetProductsByCategoryAsync(category), Times.Once);
    }

    [Fact]
    public async Task GetProductsWithStock_ReturnsOkWithProducts()
    {
        // Arrange
        var products = TestDataHelper.CreateTestProducts(2);
        _mockService.Setup(s => s.GetProductsWithStockAsync())
                   .ReturnsAsync(products);

        // Act
        var result = await _controller.GetProductsWithStock();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductResponse>>(okResult.Value);
        Assert.Equal(2, returnedProducts.Count());
        _mockService.Verify(s => s.GetProductsWithStockAsync(), Times.Once);
    }

    [Fact]
    public async Task GetProductsWithLowStock_DefaultThreshold_ReturnsOkWithProducts()
    {
        // Arrange
        var products = TestDataHelper.CreateTestProducts(1);
        _mockService.Setup(s => s.GetProductsWithLowStockAsync(10))
                   .ReturnsAsync(products);

        // Act
        var result = await _controller.GetProductsWithLowStock();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductResponse>>(okResult.Value);
        Assert.Single(returnedProducts);
        _mockService.Verify(s => s.GetProductsWithLowStockAsync(10), Times.Once);
    }

    [Fact]
    public async Task GetProductsWithLowStock_CustomThreshold_ReturnsOkWithProducts()
    {
        // Arrange
        var threshold = 5;
        var products = TestDataHelper.CreateTestProducts(1);
        _mockService.Setup(s => s.GetProductsWithLowStockAsync(threshold))
                   .ReturnsAsync(products);

        // Act
        var result = await _controller.GetProductsWithLowStock(threshold);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductResponse>>(okResult.Value);
        Assert.Single(returnedProducts);
        _mockService.Verify(s => s.GetProductsWithLowStockAsync(threshold), Times.Once);
    }
}

