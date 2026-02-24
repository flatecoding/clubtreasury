using FluentValidation;
using ClubTreasury.Data.Export;

namespace ClubTreasury.Infrastructure.DependencyInjection;

public static class ValidationRegistration
{
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<ExportModelValidator>();
        return services;
    }
}