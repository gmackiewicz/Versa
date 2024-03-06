using System.IO;
using System.Net.Mime;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Versa;

public class VersaMiddleware
{
    private readonly RequestDelegate _next;

    public VersaMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await using var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Versa.Pages.Main.html");
        using var streamReader = new StreamReader(resourceStream);
        var resourceContent = await streamReader.ReadToEndAsync();

        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = MediaTypeNames.Text.Html;

        await context.Response.WriteAsync(resourceContent);

        // var buffer = Encoding.UTF8.GetBytes(resourceContent);
        // context.Response.ContentLength = buffer.Length;
        // await using var stream = context.Response.Body;
        // await stream.WriteAsync(buffer, 0, buffer.Length);
        // await stream.FlushAsync();
    }
}
