using System.Globalization;
using Microsoft.JSInterop;
using TTCCashRegister.Data.Source;

namespace TTCCashRegister.Data.Culture;

public sealed class CultureService(IJSRuntime js) : ICultureService
{
    public CultureInfo CurrentCulture
        => CultureInfo.CurrentUICulture;

    public IReadOnlyList<CultureInfo> SupportedCultures
        => SupportedAppCultures.AllCultureInfos;

    public bool IsSupported(string cultureName)
        => SupportedAppCultures.All.Contains(cultureName);

    public async Task SetCultureAsync(string cultureName)
    {
        if (!IsSupported(cultureName))
            throw new ArgumentException($"Unsupported culture: {cultureName}");

        var cookieValue =
            $".AspNetCore.Culture=c={cultureName}|uic={cultureName}; path=/; max-age=31536000";

        await js.InvokeVoidAsync("setCultureCookie", cookieValue);
        await js.InvokeVoidAsync("setCultureAndReload", cookieValue);
    }
}