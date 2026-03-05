using Desktopcafe.Core.Enums;

namespace Desktopcafe.Core.DTOs;

public record SaleDto(
    int Id,
    string EmployeeName,
    string? ClientName,
    decimal Total,
    PaymentMethod PaymentMethod,
    DateTime CreatedAt,
    List<SaleItemDto> Items);

public record SaleItemDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal);

public record CreateSaleDto(
    int? ClientId,
    int? SessionId,
    PaymentMethod PaymentMethod,
    decimal? AmountPaid,
    List<CreateSaleItemDto> Items);

public record CreateSaleItemDto(
    int ProductId,
    int Quantity);
