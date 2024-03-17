using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Versa.Sample.MvcApp.Pages;

public class About : PageModel
{
    public void OnGet()
    {
        Message = DateTime.Now.ToLongTimeString();
    }

    public string? Message { get; set; }
}
