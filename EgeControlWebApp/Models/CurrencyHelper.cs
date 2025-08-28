namespace EgeControlWebApp.Models
{
    public static class CurrencyHelper
    {
        public static string GetCurrencySymbol(string currency)
        {
            return currency switch
            {
                "EUR" => "€",
                "TRY" => "₺",
                "USD" => "$",
                _ => currency
            };
        }

        public static string GetCurrencyName(string currency)
        {
            return currency switch
            {
                "EUR" => "Euro",
                "TRY" => "Türk Lirası",
                "USD" => "Amerikan Doları",
                _ => currency
            };
        }

        public static string FormatCurrency(decimal amount, string currency)
        {
            var symbol = GetCurrencySymbol(currency);
            return currency switch
            {
                "EUR" => $"{amount:N2} {symbol}",
                "USD" => $"{symbol}{amount:N2}",
                "TRY" => $"{amount:N2} {symbol}",
                _ => $"{amount:N2} {currency}"
            };
        }

        public static Dictionary<string, string> GetCurrencyOptions()
        {
            return new Dictionary<string, string>
            {
                { "EUR", "Euro (€)" },
                { "TRY", "Türk Lirası (₺)" },
                { "USD", "Amerikan Doları ($)" }
            };
        }
    }
}
