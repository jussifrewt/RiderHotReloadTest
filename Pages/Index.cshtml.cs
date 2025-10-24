using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace RiderHotReloadTest.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    // ВОТ ЭТА СТРОЧКА
    public string Message { get; private set; } = string.Empty;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        // И ВОТ ЭТА СТРОЧКА
        Message = "Это изначальная логика.";
    }
}