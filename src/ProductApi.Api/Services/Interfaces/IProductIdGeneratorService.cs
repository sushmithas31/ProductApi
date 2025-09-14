namespace ProductApi.Api.Services.Interfaces;

public interface IProductIdGeneratorService
{
    Task<int> GenerateNextProductIdAsync();
}

