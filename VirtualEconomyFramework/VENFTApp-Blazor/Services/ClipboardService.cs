using System.Threading.Tasks;
using Microsoft.JSInterop;

public sealed class ClipboardService
{
    private readonly IJSRuntime _jsRuntime;

    public ClipboardService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public ValueTask<string> ReadTextAsync()
    {
        return _jsRuntime.InvokeAsync<string>("navigator.clipboard.readText");
    }

    public ValueTask WriteTextAsync(string text)
    {
        return _jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
    }
}