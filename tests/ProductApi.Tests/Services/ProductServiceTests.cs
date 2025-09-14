using Microsoft.Extensions.Logging;
using Moq;
using ProductApi.Api.Models;
using ProductApi.Api.Repositories.Interfaces;
using ProductApi.Api.Services.Implementations;
using ProductApi.Api.Services.Interfaces;
using ProductApi.Tests.Helpers;
using Xunit;

namespace ProductApi.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<ILogger<ProductService>> _mockLogger;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockLogger = new Mock<ILogger<ProductService>>();
        _service = new ProductService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetProductByIdAsync_ExistingProduct_ReturnsProduct()
    {
        // Arrange
        var productId = 100001;
        var expectedProduct = TestDataHelper.CreateTestProduct(productId);
        _mockRepository.Setup(r => r.GetByIdAsync(productId))
                      .ReturnsAsync(expectedProduct);

        // Act
        var result = await _service.GetProductByIdAsync(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedProduct.ProductId, result.ProductId);
        Assert.Equal(expectedProduct.Name, result.Name);
        _mockRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
    }

    [Fact]
    public async Task GetProductByIdAsync_NonExistingProduct_ReturnsNull()
    {
        // Arrange
        var productId = 999999;
        _mockRepository.Setup(r => r.GetByIdAsync(productId))
                      .ReturnsAsync((Product?)null);

        // Act
        var result = await _service.GetProductByIdAsync(productId);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
    }

    [Fact]
    public async Task GetAllProductsAsync_ReturnsAllProducts()
    {
        // Arrange
        var expectedProducts = TestDataHelper.CreateTestProducts(3);
        _mockRepository.Setup(r => r.GetAllAsync())
                      .ReturnsAsync(expectedProducts);

        // Act
        var result = await _service.GetAllProductsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_ValidProduct_ReturnsCreatedProduct()
    {
        // Arrange
        var product = TestDataHelper.CreateTestProduct();
        product.ProductId = 0; // Simulate new product
        var createdProduct = TestDataHelper.CreateTestProduct(100001);
        
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Product>()))
                      .ReturnsAsync(createdProduct);

        // Act
        var result = await _service.CreateProductAsync(product);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdProduct.ProductId, result.ProductId);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task CreateProductAsync_InvalidName_ThrowsArgumentException(string invalidName)
    {
        // Arrange
        var product = TestDataHelper.CreateTestProduct();
        product.Name = invalidName;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateProductAsync(product));
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task CreateProductAsync_InvalidPrice_ThrowsArgumentException()
    {
        // Arrange
        var product = TestDataHelper.CreateTestProduct();
        product.Price = -10;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateProductAsync(product));
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task CreateProductAsync_NegativeStock_ThrowsArgumentException()
    {
        // Arrange
        var product = TestDataHelper.CreateTestProduct();
        product.StockAvailable = -5;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateProductAsync(product));
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProductAsync_ExistingProduct_ReturnsUpdatedProduct()
    {
        // Arrange
        var productId = 100001;
        var existingProduct = TestDataHelper.CreateTestProduct(productId);
        var updateProduct = TestDataHelper.CreateTestProduct();
        updateProduct.Name = "Updated Name";
        
        _mockRepository.Setup(r => r.GetByIdAsync(productId))
                      .ReturnsAsync(existingProduct);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>()))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateProductAsync(productId, updateProduct);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        _mockRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_NonExistingProduct_ReturnsNull()
    {
        // Arrange
        var productId = 999999;
        var updateProduct = TestDataHelper.CreateTestProduct();
        
        _mockRepository.Setup(r => r.GetByIdAsync(productId))
                      .ReturnsAsync((Product?)null);

        // Act
        var result = await _service.UpdateProductAsync(productId, updateProduct);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProductAsync_ExistingProduct_ReturnsTrue()
    {
        // Arrange
        var productId = 100001;
        var existingProduct = TestDataHelper.CreateTestProduct(productId);
        
        _mockRepository.Setup(r => r.GetByIdAsync(productId))
                      .ReturnsAsync(existingProduct);
        _mockRepository.Setup(r => r.DeleteAsync(existingProduct))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteProductAsync(productId);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(existingProduct), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_NonExistingProduct_ReturnsFalse()
    {
        // Arrange
        var productId = 999999;
        
        _mockRepository.Setup(r => r.GetByIdAsync(productId))
                      .ReturnsAsync((Product?)null);

        // Act
        var result = await _service.DeleteProductAsync(productId);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task DecrementStockAsync_ValidRequest_ReturnsTrue()
    {
        // Arrange
        var productId = 100001;
        var quantity = 5;
        
        _mockRepository.Setup(r => r.DecrementStockAsync(productId, quantity))
                      .ReturnsAsync(true);

        // Act
        var result = await _service.DecrementStockAsync(productId, quantity);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.DecrementStockAsync(productId, quantity), Times.Once);
    }

    [Fact]
    public async Task DecrementStockAsync_InvalidQuantity_ThrowsArgumentException()
    {
        // Arrange
        var productId = 100001;
        var invalidQuantity = -5;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.DecrementStockAsync(productId, invalidQuantity));
        _mockRepository.Verify(r => r.DecrementStockAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task AddToStockAsync_ValidRequest_ReturnsTrue()
    {
        // Arrange
        var productId = 100001;
        var quantity = 10;
        
        _mockRepository.Setup(r => r.AddToStockAsync(productId, quantity))
                      .ReturnsAsync(true);

        // Act
        var result = await _service.AddToStockAsync(productId, quantity);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.AddToStockAsync(productId, quantity), Times.Once);
    }

    [Fact]
    public async Task AddToStockAsync_InvalidQuantity_ThrowsArgumentException()
    {
        // Arrange
        var productId = 100001;
        var invalidQuantity = 0;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddToStockAsync(productId, invalidQuantity));
        _mockRepository.Verify(r => r.AddToStockAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetProductsByCategoryAsync_ValidCategory_ReturnsProducts()
    {
        // Arrange
        var category = "Electronics";
        var expectedProducts = TestDataHelper.CreateTestProducts(2);
        
        _mockRepository.Setup(r => r.GetProductsByCategoryAsync(category))
                      .ReturnsAsync(expectedProducts);

        // Act
        var result = await _service.GetProductsByCategoryAsync(category);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockRepository.Verify(r => r.GetProductsByCategoryAsync(category), Times.Once);
    }

    [Fact]
    public async Task ProductExistsAsync_ExistingProduct_ReturnsTrue()
    {
        // Arrange
        var productId = 100001;
        
        _mockRepository.Setup(r => r.ExistsAsync(productId))
                      .ReturnsAsync(true);

        // Act
        var result = await _service.ProductExistsAsync(productId);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.ExistsAsync(productId), Times.Once);
    }

    [Fact]
    public async Task ProductExistsAsync_NonExistingProduct_ReturnsFalse()
    {
        // Arrange
        var productId = 999999;
        
        _mockRepository.Setup(r => r.ExistsAsync(productId))
                      .ReturnsAsync(false);

        // Act
        var result = await _service.ProductExistsAsync(productId);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.ExistsAsync(productId), Times.Once);
    }
}

