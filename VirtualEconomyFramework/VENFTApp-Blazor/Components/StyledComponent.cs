using Microsoft.AspNetCore.Components;

public abstract class StyledComponent : ComponentBase
{
    [Parameter]
    public string Style { get; set; }
    [Parameter]
    public string Class { get; set; }
    [Parameter]
    public RenderFragment ChildContent { get; set; }
}

public abstract class StyledComponent<T> : ComponentBase
{
    [Parameter]
    public string Style { get; set; }
    [Parameter]
    public string Class { get; set; }
    [Parameter]
    public RenderFragment<T> ChildContent { get; set; }
}
