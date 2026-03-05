using Desktopcafe.Core.DTOs;
using Desktopcafe.Core.Enums;
using FluentValidation;

namespace Desktopcafe.Shared.Validators;

public class CreateSessionValidator : AbstractValidator<CreateSessionDto>
{
    public CreateSessionValidator()
    {
        RuleFor(x => x.ComputerId).GreaterThan(0).WithMessage("Seleccione una computadora");
        RuleFor(x => x.RatePerHour).GreaterThan(0).WithMessage("La tarifa debe ser mayor a 0");
        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .When(x => x.SessionType == SessionType.Prepaid)
            .WithMessage("La duracion debe ser mayor a 0 para sesiones prepago");
    }
}

public class ExtendSessionValidator : AbstractValidator<ExtendSessionDto>
{
    public ExtendSessionValidator()
    {
        RuleFor(x => x.SessionId).GreaterThan(0);
        RuleFor(x => x.AdditionalMinutes)
            .GreaterThan(0).WithMessage("El tiempo adicional debe ser mayor a 0")
            .LessThanOrEqualTo(480).WithMessage("No se puede extender mas de 8 horas");
    }
}
