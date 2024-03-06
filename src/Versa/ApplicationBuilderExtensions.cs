using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Versa;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseVersaPanel(this IApplicationBuilder app, string path = "/versa")
    {
        app.Map(new PathString(path), x => x.UseMiddleware<VersaMiddleware>());

        return app;
    }
}
