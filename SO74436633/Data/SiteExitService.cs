using Microsoft.JSInterop;

namespace SO74436633;

public class SiteExitService
{
    private IJSRuntime? _js { get; set; }

    private TaskCompletionSource? _taskCompletionSource;

    public event EventHandler? SPAClosed;
    public SiteExitService(IJSRuntime? js)
        => _js = js;

    public async Task SetSpaExit()
    {
        if (_taskCompletionSource is null)
        {
            _taskCompletionSource = new TaskCompletionSource();
            var objref = DotNetObjectReference.Create(this);
            await _js!.InvokeVoidAsync("blazr_setExitCheck", objref, true);
            _taskCompletionSource.SetResult();
        }

        if (!_taskCompletionSource.Task.IsCompleted)
            await _taskCompletionSource.Task;
    }


    [JSInvokable]
    public Task SpaExit()
    {
        // do whatever you want to do on exit  Raise an event of you wish
        this.SPAClosed?.Invoke(null, EventArgs.Empty);
        return Task.CompletedTask;
    }
}
