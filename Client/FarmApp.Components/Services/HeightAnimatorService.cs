using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class HeightAnimatorService
{
    private readonly IJSRuntime _js;

    public HeightAnimatorService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task MeasureAsync(ElementReference el)
    {
        await _js.InvokeVoidAsync("heightAnimator.measure", el);
    }

    public async Task AnimateAsync(ElementReference el)
    {
        await _js.InvokeVoidAsync("heightAnimator.animate", el);
    }

    public async Task TransitionAsync(ElementReference el, Func<Task> update)
    {
        await MeasureAsync(el);

        await update();

        await Task.Yield();

        await AnimateAsync(el);
    }
}