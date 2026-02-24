namespace ClubTreasury.Data.Culture;

using System.Globalization;

public interface ICultureService
{
    CultureInfo CurrentCulture { get; }

    IReadOnlyList<CultureInfo> SupportedCultures { get; }

    bool IsSupported(string cultureName);

    Task SetCultureAsync(string cultureName);
}
