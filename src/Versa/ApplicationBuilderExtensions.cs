using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Versa;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseVersaPanel(this IApplicationBuilder app, string path = "/versa")
    {
        app.Map(new PathString(path), x => x.UseMiddleware<VersaMiddleware>());

        return app;
    }

    public static void AddVersaPages(this IServiceCollection services)
    {
    }
}
