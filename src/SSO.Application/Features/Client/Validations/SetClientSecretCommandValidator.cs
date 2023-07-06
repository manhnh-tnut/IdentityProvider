using FluentValidation;
using SSO.Application.Features.Client.Commands;

namespace SSO.Application.Features.Client.Validations
{
    public class SetClientSecretCommandValidator : AbstractValidator<SetClientSecretCommand>
    {
        public SetClientSecretCommandValidator(ILogger<SetClientSecretCommandValidator> logger)
        {
            RuleFor(command => command.Id).NotEmpty().MinimumLength(1);
            RuleFor(command => command.Secret).NotEmpty().MinimumLength(1);
            logger.LogTrace("----- INSTANCE GET - {ClassName}", GetType().Name);
        }
    }
}
