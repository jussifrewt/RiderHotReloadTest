using Microsoft.AspNetCore.Mvc.RazorPages;
namespace RiderHotReloadTest.Pages;
public class AdvancedModel : PageModel
{
    private readonly IMessageService _messageService;
    public List<string> FilteredData { get; set; } = new();
    public string AsyncMessage { get; set; } = string.Empty;
    public string ServiceMessage { get; set; } = string.Empty;

    private readonly List<string> _sourceData = new() { "Austria", "Belgium", "Germany", "Australia", "Oman" };
    
    public AdvancedModel(IMessageService messageService)
    {
        _messageService = messageService;
    }

    public async Task OnGetAsync()
    {
        // 1. LINQ Test
        FilteredData = _sourceData.Where(f => f.StartsWith("A")).ToList();

        // 2. Async/await Test
        await Task.Delay(50);
        AsyncMessage = "Async task finished!";
        
        // 3. Dependency Injection Test
        ServiceMessage = _messageService.GetMessage();
    }
}