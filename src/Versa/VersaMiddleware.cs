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
        // TODO
        await Task.Yield();
    }
}
