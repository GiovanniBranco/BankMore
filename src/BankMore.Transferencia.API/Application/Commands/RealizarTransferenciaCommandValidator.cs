using FluentValidation;

namespace BankMore.Transferencia.API.Application.Commands;

public class RealizarTransferenciaCommandValidator : AbstractValidator<RealizarTransferenciaCommand>
{
    public RealizarTransferenciaCommandValidator()
    {
        RuleFor(x => x.IdRequisicao)
            .NotEmpty()
            .WithMessage("ID de requisição é obrigatório");

        RuleFor(x => x.IdContaOrigem)
            .GreaterThan(0)
            .WithMessage("ID da conta origem deve ser maior que zero");

        RuleFor(x => x.IdContaDestino)
            .GreaterThan(0)
            .WithMessage("ID da conta destino deve ser maior que zero");

        RuleFor(x => x.Valor)
            .GreaterThan(0)
            .WithMessage("Valor da transferência deve ser positivo");

        RuleFor(x => x)
            .Must(x => x.IdContaOrigem != x.IdContaDestino)
            .WithMessage("Conta origem e destino não podem ser iguais");
    }
}
