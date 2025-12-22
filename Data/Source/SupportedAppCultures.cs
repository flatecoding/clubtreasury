using System.Globalization;

namespace TTCCashRegister.Data.Source;

public static class SupportedAppCultures
{
        public const string English = "en-US";
        public const string German  = "de-DE";

        public static readonly string[] All =
        {
                English,
                German
        };

        public static readonly CultureInfo[] AllCultureInfos =
        {
                new(English),
                new(German)
        };
}