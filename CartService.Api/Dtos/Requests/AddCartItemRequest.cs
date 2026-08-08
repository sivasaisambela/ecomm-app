namespace CartService.Api.Dtos.Requests
{
    public record AddCartItemRequest(
     Guid ProductId,
     string ProductName,
     decimal UnitPrice,
     int Quantity);
}
