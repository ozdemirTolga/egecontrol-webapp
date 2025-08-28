using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EgeControlWebApp.Infrastructure.Binding
{
    // Accepts both comma and dot as decimal separator and strips group separators
    public sealed class DecimalModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);
            var value = valueProviderResult.FirstValue;
            if (string.IsNullOrWhiteSpace(value))
            {
                return Task.CompletedTask;
            }

            var culture = CultureInfo.CurrentCulture;
            var decSep = culture.NumberFormat.NumberDecimalSeparator;
            var grpSep = culture.NumberFormat.NumberGroupSeparator;

            // Normalize: remove grouping separators and unify decimal separator to current culture
            value = value.Replace(grpSep, string.Empty);
            if (decSep == ",")
            {
                value = value.Replace(".", ",");
            }
            else
            {
                value = value.Replace(",", ".");
            }

            if (decimal.TryParse(value, NumberStyles.Number, culture, out var result))
            {
                bindingContext.Result = ModelBindingResult.Success(result);
                return Task.CompletedTask;
            }

            // Fallback to invariant
            if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
            {
                bindingContext.Result = ModelBindingResult.Success(result);
                return Task.CompletedTask;
            }

            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, $"Geçersiz sayı: {value}");
            return Task.CompletedTask;
        }
    }
}
