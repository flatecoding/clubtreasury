using Microsoft.Extensions.Localization;
using MudBlazor;

namespace ClubTreasury.Infrastructure.Localization;

public class MudBlazorLocalizationInterceptor(
    ILoggerFactory loggerFactory,
    IStringLocalizer<Translation> localizer)
    : DefaultLocalizationInterceptor(loggerFactory)
{
    public override LocalizedString Handle(string key, params object[] arguments)
    {
        var translation = localizer[key];

        if (translation.ResourceNotFound)
            return base.Handle(key, arguments);

        var formatted = arguments.Length > 0
            ? string.Format(translation.Value, arguments)
            : translation.Value;

        return new LocalizedString(key, formatted);
    }
}