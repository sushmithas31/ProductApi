using Microsoft.EntityFrameworkCore;
using ProductApi.Api.Data;
using ProductApi.Api.Services.Interfaces;

namespace ProductApi.Api.Services.Implementations;

public class ProductIdGeneratorService : IProductIdGeneratorService
{
    private readonly ProductApiDbContext _context;
    private readonly ILogger<ProductIdGeneratorService> _logger;

    public ProductIdGeneratorService(ProductApiDbContext context, ILogger<ProductIdGeneratorService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int> GenerateNextProductIdAsync()
    {
        try
        {
            var connection = _context.Database.GetDbConnection();

            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT NEXT VALUE FOR ProductIdSequence";

            var result = await command.ExecuteScalarAsync();
            var nextId = Convert.ToInt32(result);

            _logger.LogInformation("Generated new ProductId: {ProductId}", nextId);
            return nextId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating next ProductId");
            throw;
        }
    }
}