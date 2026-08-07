using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Services;
using Shared.Core.Responses;

namespace ProductService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductApplicationService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductApplicationService productService, ILogger<ProductsController> logger)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ============================================
    // READ ENDPOINTS
    // ============================================

    /// <summary>
    /// Get all active products
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP GET: api/v1/products - Fetching all products");
        var products = await _productService.GetAllProductsAsync(cancellationToken);
        return Ok(new ApiResponse<IEnumerable<ProductDto>>(true, products));
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP GET: api/v1/products/{id} - Fetching product", id);
        var product = await _productService.GetProductByIdAsync(id, cancellationToken);
        return Ok(new ApiResponse<ProductDto>(true, product));
    }

    /// <summary>
    /// Get product by SKU
    /// </summary>
    [HttpGet("sku/{sku}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySku(string sku, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP GET: api/v1/products/sku/{sku} - Fetching product", sku);
        var product = await _productService.GetProductBySkuAsync(sku, cancellationToken);
        return Ok(new ApiResponse<ProductDto>(true, product));
    }

    /// <summary>
    /// Get products by Category
    /// </summary>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategory(string category, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP GET: api/v1/products/category/{category} - Fetching products", category);
        var products = await _productService.GetProductsByCategoryAsync(category, cancellationToken);
        return Ok(new ApiResponse<IEnumerable<ProductDto>>(true, products));
    }

    /// <summary>
    /// Get products that are low in stock
    /// </summary>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLowStock(CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP GET: api/v1/products/low-stock - Fetching low-stock products");
        var products = await _productService.GetLowStockProductsAsync(cancellationToken);
        return Ok(new ApiResponse<IEnumerable<ProductDto>>(true, products));
    }

    // ============================================
    // MUTATION ENDPOINTS (CREATE / UPDATE / DELETE)
    // ============================================

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductDto createDto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP POST: api/v1/products - Creating product: {ProductName}", createDto.Name);

        // In a real system, the user ID would be extracted from the JWT security token claims.
        // For now, we use a placeholder "system-user" or simulated user ID.
        var currentUserId = User.Identity?.Name ?? "system-user";

        var product = await _productService.CreateProductAsync(createDto, currentUserId, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            new ApiResponse<ProductDto>(true, product, "Product created successfully.")
        );
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto updateDto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP PUT: api/v1/products/{id} - Updating product", id);

        if (id != updateDto.Id)
        {
            return BadRequest(new ApiResponse<object>(false, null, "Product ID mismatch between route and payload."));
        }

        var currentUserId = User.Identity?.Name ?? "system-user";
        var updatedProduct = await _productService.UpdateProductAsync(updateDto, currentUserId, cancellationToken);

        return Ok(new ApiResponse<ProductDto>(true, updatedProduct, "Product updated successfully."));
    }

    /// <summary>
    /// Soft-delete a product
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP DELETE: api/v1/products/{id} - Soft-deleting product", id);

        var currentUserId = User.Identity?.Name ?? "system-user";
        await _productService.DeleteProductAsync(id, currentUserId, cancellationToken);

        return NoContent(); // Standard REST response for successful deletion without return payload
    }

    // ============================================
    // STOCK INVENTORY MANAGEMENT
    // ============================================

    /// <summary>
    /// Reserve stock for a product (e.g., when added to checkout)
    /// </summary>
    [HttpPost("{id:guid}/reserve-stock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReserveStock(Guid id, [FromBody] int quantity, CancellationToken cancellationToken)
    {
        await _productService.ReserveStockAsync(id, quantity, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Release reserved stock back to active inventory (e.g., when checkout is canceled)
    /// </summary>
    [HttpPost("{id:guid}/release-stock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReleaseStock(Guid id, [FromBody] int quantity, CancellationToken cancellationToken)
    {
        await _productService.ReleaseReservedStockAsync(id, quantity, cancellationToken);
        return NoContent();
    }
}

// A standard helper wrapper for unified JSON responses across our microservices
