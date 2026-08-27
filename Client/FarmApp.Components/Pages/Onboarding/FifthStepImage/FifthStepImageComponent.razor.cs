using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Implementation;

namespace FarmApp.Components.Pages.Onboarding.FifthStepImage
{
    public partial class FifthStepImageComponent
    {
        [Parameter] public bool Animate { get; set; }

        [Parameter] public EventCallback OnAnimationComplete { get; set; }

        [Inject] private IJSRuntime JSRuntime { get; set; } = null!;

        private FifthStepIcon[] _icons;

        private DotNetObjectReference<FifthStepImageComponent> _objRef;

        private IJSObjectReference _module;

        private ElementReference _orbitRing;
        private ElementReference _centerIcon;
        private ElementReference _leafIcon;
        private ElementReference[] _iconRefs;

        private bool _animationStarted = false;

        protected override void OnInitialized()
        {
            var iconDefs = new[]
            {
                ("shared/marker-02.svg",           "#C42921", "#FFDED8"),
                ("shared/leaf.svg",                "#E7641D", "#FFEEDB"),
                ("settings/globus.svg",            "#5D21CE", "#EEE7FF"),
                ("settings/3-balls.svg",           "#006848", "#ECF9F3"),
                ("user-notifications/alarm-01.svg","#0364C5", "#E6F3FF"),
                ("weather/sun-03.svg",             "#E35002", "#FFEEDB"),
                ("notes/calendar-02.svg",          "#5D21CE", "#EEE7FF"),
                ("shared/bell.svg",                "#006848", "#ECF9F3"),
            };

            int total = iconDefs.Length;
            double radius = 111;
            int containerSize = 272;
            int iconSize = 50;
            int center = containerSize / 2 - iconSize / 2;

            _icons = iconDefs.Select((def, i) =>
            {
                double angle = (2 * Math.PI / total) * i - Math.PI / 2;
                int left = (int)Math.Round(center + radius * Math.Cos(angle));
                int top = (int)Math.Round(center + radius * Math.Sin(angle));
                return new FifthStepIcon(def.Item1, def.Item2, def.Item3, top, left, Index: i);
            }).ToArray();

            _iconRefs = new ElementReference[_icons.Length];
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _objRef = DotNetObjectReference.Create(this);

                _module = await JSRuntime.InvokeAsync<IJSObjectReference>("import",
                    "./_content/FarmApp.Components/Pages/Onboarding/FifthStepImage/FifthStepImageComponent.razor.js");

                await _module.InvokeVoidAsync("startOrbit", _orbitRing, _iconRefs);
            }

            if (Animate && _module != null && !_animationStarted)
            {
                _animationStarted = true;
                await _module.InvokeVoidAsync("startSuckIn", _orbitRing, _centerIcon, _iconRefs, _leafIcon, _objRef);
            }
        }

        [JSInvokable]
        public async Task NotifyAnimationFinished()
        {
            await OnAnimationComplete.InvokeAsync();
        }

        public async ValueTask DisposeAsync()
        {
            _objRef?.Dispose();
            if (_module is not null) await _module.DisposeAsync();
        }
    }
}

public record FifthStepIcon(string Src, string Color, string BgColor, int top, int left, int Index);