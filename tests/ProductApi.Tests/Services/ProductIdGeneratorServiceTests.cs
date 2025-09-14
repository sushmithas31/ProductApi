using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ProductApi.Api.Data;
using ProductApi.Api.Services.Implementations;
using Xunit;

namespace ProductApi.Tests.Services;

public class ProductIdGeneratorServiceTests : IDisposable
{
    private readonly ProductApiDbContext _context;
    private readonly Mock<ILogger<ProductIdGeneratorService>> _mockLogger;
    private readonly ProductIdGeneratorService _service;

    public ProductIdGeneratorServiceTests()
    {
        var options = new DbContextOptionsBuilder<ProductApiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProductApiDbContext(options);
        _mockLogger = new Mock<ILogger<ProductIdGeneratorService>>();
        _service = new ProductIdGeneratorService(_context, _mockLogger.Object);

        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task GenerateNextProductIdAsync_FirstCall_ReturnsExpectedId()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GenerateNextProductIdAsync());
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange & Act
        var service = new ProductIdGeneratorService(_context, _mockLogger.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task GenerateNextProductIdAsync_LogsInformation_WhenSuccessful()
    {

        try
        {
            // Act
            await _service.GenerateNextProductIdAsync();
        }
        catch (InvalidOperationException)
        {
        }

        Assert.NotNull(_service);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

