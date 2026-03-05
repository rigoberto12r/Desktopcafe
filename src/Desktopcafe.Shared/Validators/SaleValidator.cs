using Desktopcafe.Core.DTOs;
using FluentValidation;

namespace Desktopcafe.Shared.Validators;

public class CreateSaleValidator : AbstractValidator<CreateSaleDto>
{
    public CreateSaleValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("La venta debe tener al menos un producto");
        RuleFor(x => x.PaymentMethod).IsInEnum().WithMessage("Metodo de pago invalido");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).GreaterThan(0);
            item.RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0");
        });
    }
}
