using Desktopcafe.Core.DTOs;
using Desktopcafe.Core.Enums;
using Desktopcafe.Shared.Validators;
using FluentAssertions;
using Xunit;

namespace Desktopcafe.Server.Tests;

public class ValidatorTests
{
    [Fact]
    public void CreateSession_Valid_Prepaid_Passes()
    {
        var validator = new CreateSessionValidator();
        var dto = new CreateSessionDto(1, null, SessionType.Prepaid, 60, 20m);
        var result = validator.Validate(dto);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateSession_Prepaid_NoDuration_Fails()
    {
        var validator = new CreateSessionValidator();
        var dto = new CreateSessionDto(1, null, SessionType.Prepaid, null, 20m);
        var result = validator.Validate(dto);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateSession_Free_NoDuration_Passes()
    {
        var validator = new CreateSessionValidator();
        var dto = new CreateSessionDto(1, null, SessionType.Free, null, 20m);
        var result = validator.Validate(dto);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateSession_NoComputer_Fails()
    {
        var validator = new CreateSessionValidator();
        var dto = new CreateSessionDto(0, null, SessionType.Prepaid, 60, 20m);
        var result = validator.Validate(dto);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ExtendSession_Valid_Passes()
    {
        var validator = new ExtendSessionValidator();
        var dto = new ExtendSessionDto(1, 30);
        var result = validator.Validate(dto);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ExtendSession_TooLong_Fails()
    {
        var validator = new ExtendSessionValidator();
        var dto = new ExtendSessionDto(1, 500);
        var result = validator.Validate(dto);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateSale_NoItems_Fails()
    {
        var validator = new CreateSaleValidator();
        var dto = new CreateSaleDto(null, null, PaymentMethod.Cash, null, new List<CreateSaleItemDto>());
        var result = validator.Validate(dto);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateSale_WithItems_Passes()
    {
        var validator = new CreateSaleValidator();
        var dto = new CreateSaleDto(null, null, PaymentMethod.Cash, 50m,
            new List<CreateSaleItemDto> { new(1, 2), new(3, 1) });
        var result = validator.Validate(dto);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateSale_ZeroQuantity_Fails()
    {
        var validator = new CreateSaleValidator();
        var dto = new CreateSaleDto(null, null, PaymentMethod.Cash, null,
            new List<CreateSaleItemDto> { new(1, 0) });
        var result = validator.Validate(dto);
        result.IsValid.Should().BeFalse();
    }
}
