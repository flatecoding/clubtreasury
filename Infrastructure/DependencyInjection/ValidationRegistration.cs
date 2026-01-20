using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TTCCashRegister.Data.Export;

namespace TTCCashRegister.Infrastructure.DependencyInjection;

public static class ValidationRegistration
{
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<ExportModelValidator>();
        return services;
    }
}