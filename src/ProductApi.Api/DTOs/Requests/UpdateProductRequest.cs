namespace ProductApi.Api.DTOs.Requests;

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockAvailable { get; set; }
    public string Category { get; set; } = string.Empty;
}

