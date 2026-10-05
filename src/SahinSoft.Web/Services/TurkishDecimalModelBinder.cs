using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace SahinSoft.Web.Services;

// Sunucu kültürü en-US olduğundan "125,50" varsayılan binder'da 12550 oluyordu (virgül binlik ayırıcı sayılır) —
// istemci betiği değeri normalleştirmeyen her formda 100 kat büyüme riski. Bu binder Türkçe girişi doğru okur:
//   "1.250,50" -> 1250.50 · "125,50" -> 125.50 · "1250.50" -> 1250.50 (betiğin normalleştirdiği biçim) · "1250" -> 1250
// Kural: iki ayırıcı da varsa SONUNCUSU ondalıktır; yalnız virgül varsa ondalık virgüldür; yalnız nokta varsa ondalık noktadır.
public sealed class TurkishDecimalModelBinder(Type modelType) : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (result == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, result);
        var raw = result.FirstValue?.Trim();
        var nullable = Nullable.GetUnderlyingType(modelType) is not null;

        if (string.IsNullOrEmpty(raw))
        {
            if (nullable) { bindingContext.Result = ModelBindingResult.Success(null); }
            else { bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Sayısal bir değer giriniz."); }
            return Task.CompletedTask;
        }

        if (TryParse(raw, out var value))
        {
            bindingContext.Result = ModelBindingResult.Success(value);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Geçerli bir sayı giriniz.");
        }
        return Task.CompletedTask;
    }

    public static bool TryParse(string raw, out decimal value)
    {
        var s = raw.Replace(" ", "").Replace(" ", "");
        var lastComma = s.LastIndexOf(',');
        var lastDot = s.LastIndexOf('.');
        if (lastComma >= 0 && lastDot >= 0)
        {
            var decimalSep = lastComma > lastDot ? ',' : '.';
            var groupSep = decimalSep == ',' ? '.' : ',';
            s = s.Replace(groupSep.ToString(), "");
            if (decimalSep == ',') { s = s.Replace(',', '.'); }
        }
        else if (lastComma >= 0)
        {
            s = s.Replace(',', '.');
        }

        return decimal.TryParse(s, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value);
    }
}

public sealed class TurkishDecimalModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        var type = context.Metadata.ModelType;
        return type == typeof(decimal) || type == typeof(decimal?) ? new TurkishDecimalModelBinder(type) : null;
    }
}
