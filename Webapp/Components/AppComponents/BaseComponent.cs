using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Webapp.Components.AppComponents;

public abstract class BaseComponent:ComponentBase {
    [Inject] protected NavigationManager NavigationManager { get; set; }

    protected override Task OnInitializedAsync() {
        this.NavigationManager.LocationChanged+=LocationChanged;
        return base.OnInitializedAsync();
    }
    
    private void LocationChanged(object? sender, LocationChangedEventArgs e) {

    }
}