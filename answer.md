> Is there a way to detect when a user closes the browser in Blazor Server?

You need to set up a listener on the browser `beforeunload` event to make a callback into Blazor.

First some JS.

```js
// site.js
// load in _Layout_.cshtml
window.blazr_setExitCheck = function (dotNetHelper, set) {
    if (set) {
        window.addEventListener("beforeunload", blazr_spaExit);
        blazrDotNetExitHelper = dotNetHelper;
    }
    else {
        window.removeEventListener("beforeunload", blazr_spaExit);
        blazrDotNetExitHelper = null;
    }
}

var blazrDotNetExitHelper;

window.blazr_spaExit = function (event) {
    event.preventDefault();
    blazrDotNetExitHelper.invokeMethodAsync("SpaExit");
}
```

A SiteExitService.  Your run whatever code you want in `SpaExit` or register an event handler from elsewhere on `SPAClosed`.

```csharp
public class SiteExitService
{
    private IJSRuntime? _js { get; set; }

    private TaskCompletionSource? _taskCompletionSource;

    public event EventHandler? SPAClosed;

    public SiteExitService(IJSRuntime? js)
        => _js = js;

    public async Task SetSpaExit()
    {
        // makes sure we only do it once
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
        // do whatever you want to do on exit  Raise an event if you wish
        this.SPAClosed?.Invoke(null, EventArgs.Empty);
        return Task.CompletedTask;
    }
}
```

Program:
```csharp
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddScoped<SiteExitService>();
```

Set up in App so it's always loaded.

```csharp
// <Router AppAssembly="@typeof(App).Assembly">
// ...
//</Router>
@code {
    [Inject] private SiteExitService Service { get; set; } = default!;

    protected async override Task OnAfterRenderAsync(bool firstRender)
        => await Service.SetSpaExit();
}
```

Check it works by putting a breakpoint on `this.SPAClosed?.Invoke(null, EventArgs.Empty);`.
