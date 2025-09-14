using Microsoft.AspNetCore.Mvc;
using ProductApi.Api.DTOs.Requests;
using ProductApi.Api.DTOs.Responses;
using ProductApi.Api.Extensions;
using ProductApi.Api.Services.Interfaces;

namespace ProductApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    /// <returns>List of all products with stock information</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAllProducts()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            var response = products.ToResponse();
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all products");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product with stock information</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductResponse>> GetProductById(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound($"Product with ID {id} not found");
            }

            var response = product.ToResponse();
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting product with ID {ProductId}", id);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="request">Product creation request</param>
    /// <returns>Created product with generated ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductResponse>> CreateProduct([FromBody] CreateProductRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = request.ToEntity();
            var createdProduct = await _productService.CreateProductAsync(product);
            var response = createdProduct.ToResponse();

            return CreatedAtAction(
                nameof(GetProductById),
                new { id = createdProduct.ProductId },
                response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument provided for product creation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating product");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="request">Product update request</param>
    /// <returns>Updated product</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductResponse>> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = request.ToEntity();
            var updatedProduct = await _productService.UpdateProductAsync(id, product);

            if (updatedProduct == null)
            {
                return NotFound($"Product with ID {id} not found");
            }

            var response = updatedProduct.ToResponse();
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument provided for product update with ID {ProductId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating product with ID {ProductId}", id);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result)
            {
                return NotFound($"Product with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting product with ID {ProductId}", id);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Decrement product stock
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="quantity">Quantity to decrement</param>
    /// <returns>Updated product with new stock level</returns>
    [HttpPut("decrement-stock/{id}/{quantity}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductResponse>> DecrementStock(int id, int quantity)
    {
        try
        {
            if (quantity <= 0)
            {
                return BadRequest("Quantity must be greater than zero");
            }

            var result = await _productService.DecrementStockAsync(id, quantity);
            if (!result)
            {
                return BadRequest($"Unable to decrement stock for product {id}. Product not found or insufficient stock.");
            }

            // Get updated product to return
            var updatedProduct = await _productService.GetProductByIdAsync(id);
            if (updatedProduct == null)
            {
                return NotFound($"Product with ID {id} not found");
            }

            var response = updatedProduct.ToResponse();
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument provided for stock decrement");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while decrementing stock for product {ProductId}", id);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Add to product stock
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="quantity">Quantity to add</param>
    /// <returns>Updated product with new stock level</returns>
    [HttpPut("add-to-stock/{id}/{quantity}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductResponse>> AddToStock(int id, int quantity)
    {
        try
        {
            if (quantity <= 0)
            {
                return BadRequest("Quantity must be greater than zero");
            }

            var result = await _productService.AddToStockAsync(id, quantity);
            if (!result)
            {
                return NotFound($"Product with ID {id} not found");
            }

            // Get updated product to return
            var updatedProduct = await _productService.GetProductByIdAsync(id);
            if (updatedProduct == null)
            {
                return NotFound($"Product with ID {id} not found");
            }

            var response = updatedProduct.ToResponse();
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument provided for stock addition");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding stock for product {ProductId}", id);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    /// <param name="category">Product category</param>
    /// <returns>List of products in the specified category</returns>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProductsByCategory(string category)
    {
        try
        {
            var products = await _productService.GetProductsByCategoryAsync(category);
            var response = products.ToResponse();
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting products by category {Category}", category);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Get products with available stock
    /// </summary>
    /// <returns>List of products with stock greater than zero</returns>
    [HttpGet("with-stock")]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProductsWithStock()
    {
        try
        {
            var products = await _productService.GetProductsWithStockAsync();
            var response = products.ToResponse();
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting products with stock");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Get products with low stock
    /// </summary>
    /// <param name="threshold">Stock threshold (default: 10)</param>
    /// <returns>List of products with stock below or equal to threshold</returns>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProductsWithLowStock([FromQuery] int threshold = 10)
    {
        try
        {
            var products = await _productService.GetProductsWithLowStockAsync(threshold);
            var response = products.ToResponse();
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting products with low stock");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
}

