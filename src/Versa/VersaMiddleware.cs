using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Versa.Pages;

namespace Versa;

public class VersaMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public VersaMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
    {
        _next = next;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var pageExecutor = scope.ServiceProvider.GetRequiredService<PageResultExecutor>();

        var pageModel = new Main();

        pageModel.OnGet();

        var routeData = new RouteData();
        routeData.Values["page"] = "/Pages/Main.cshtml";

        var actionContext = new ActionContext(context, routeData, new ActionDescriptor());
        var viewData = new ViewDataDictionary<Main>(new EmptyModelMetadataProvider(), new ModelStateDictionary());
        viewData.Model = pageModel;
        var pageContext = new PageContext(actionContext)
        {
            ViewData = viewData
        };

        var result = new PageResult();
        await pageExecutor.ExecuteAsync(pageContext, result);
    }
}
