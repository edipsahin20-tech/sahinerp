using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

namespace SahinSoft.Web.Filters;

public sealed class ConcurrencyExceptionFilter(ITempDataDictionaryFactory tempDataFactory) : IAsyncExceptionFilter
{
    public Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is not DbUpdateConcurrencyException)
        {
            return Task.CompletedTask;
        }

        var tempData = tempDataFactory.GetTempData(context.HttpContext);
        tempData["Error"] = "Bu kayıt siz düzenlerken başka bir kullanıcı tarafından değiştirilmiş. Lütfen sayfayı yenileyip tekrar deneyin.";

        var redirectUrl = "/";
        var referer = context.HttpContext.Request.Headers.Referer.ToString();
        if (!string.IsNullOrEmpty(referer)
            && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri)
            && refererUri.Host == context.HttpContext.Request.Host.Host)
        {
            redirectUrl = refererUri.PathAndQuery;
        }

        context.Result = new RedirectResult(redirectUrl);
        context.ExceptionHandled = true;

        return Task.CompletedTask;
    }
}
