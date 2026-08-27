using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using FarmApp.Services.Models;
using FarmApp.Services.Services.Interfaces;

namespace FarmApp.Components.Routing.RouteView;

public class FlexibleRouteView : IComponent
{
    [Inject] private INavigationService NavigationService { get; set; }
    
    [Parameter] public RouteDataModel RouteData { get; set; }
    [Parameter] public Type DefaultLayout { get; set; }
    
    private readonly RenderFragment _renderDelegate;
    private readonly RenderFragment _renderPageWithParametersDelegate;
    private RenderHandle _renderHandle;
    
    public FlexibleRouteView()
    {
        _renderDelegate = Render;
        _renderPageWithParametersDelegate = RenderPageWithParameters;
    }

    public void Attach(RenderHandle renderHandle)
    {
        _renderHandle = renderHandle;
    }

    public Task SetParametersAsync(ParameterView parameters)
    {
        parameters.SetParameterProperties(this);

        if (RouteData == null)
        {
            throw new InvalidOperationException($"The {nameof(FlexibleRouteView)} component requires a non-null value for the parameter {nameof(RouteData)}.");
        }

        _renderHandle.Render(_renderDelegate);
        return Task.CompletedTask;
    }

    protected virtual void Render(RenderTreeBuilder builder)
    {
        var pageLayoutType = RouteData.PageType.GetCustomAttribute<LayoutAttribute>()?.LayoutType
            ?? DefaultLayout;

        builder.OpenComponent<LayoutView>(0);
        builder.AddAttribute(1, nameof(LayoutView.Layout), pageLayoutType);
        builder.AddAttribute(2, nameof(LayoutView.ChildContent), _renderPageWithParametersDelegate);
        builder.CloseComponent();
    }

    private void RenderPageWithParameters(RenderTreeBuilder builder)
    {
        builder.OpenComponent(0, RouteData.PageType);

        foreach (var kvp in RouteData.Parameters)
        {
            builder.AddAttribute(1, kvp.Key, kvp.Value);
        }
        
        // var queryParameterSupplier = QueryParameterValueSupplier.ForType(RouteData.PageType);
        // if (queryParameterSupplier is not null)
        // {
        //     // Since this component does accept some parameters from query, we must supply values for all of them,
        //     // even if the querystring in the URI is empty. So don't skip the following logic.
        //     var url = NavigationManager.Uri;
        //     ReadOnlyMemory<char> query = default;
        //     var queryStartPos = url.IndexOf('?');
        //     if (queryStartPos >= 0)
        //     {
        //         var queryEndPos = url.IndexOf('#', queryStartPos);
        //         query = url.AsMemory(queryStartPos..(queryEndPos < 0 ? url.Length : queryEndPos));
        //     }
        //     queryParameterSupplier.RenderParametersFromQueryString(builder, query);
        // }

        builder.CloseComponent();
    }
}