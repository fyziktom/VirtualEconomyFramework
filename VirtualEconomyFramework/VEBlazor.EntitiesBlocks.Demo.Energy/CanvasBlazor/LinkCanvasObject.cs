using Blazor.Extensions.Canvas.Canvas2D;
using Blazorise;
using Microsoft.AspNetCore.Components;

namespace VEBlazor.EntitiesBlocks.Demo.Energy.CanvasBlazor;

public class LinkCanvasObject   
    : CanvasObject
{
    private readonly ILinkableCanvasObject _source;
    private readonly ILinkableCanvasObject _target;


    public LinkCanvasObject(ILinkableCanvasObject source, ILinkableCanvasObject target)
    {
        _source = source;
        _target = target;
    }

    public override async ValueTask Render(Canvas2DContext canvas2DContext, bool isHighlighted, float aspect)
    {
        var linkSourcePoint = _source.GetLinkSourcePoint();
        var linkTargetPoint = _target.GetLinkTargetPoint();

        await canvas2DContext.SetLineDashAsync(new[] {0f});
        await canvas2DContext.SetLineWidthAsync(1);
        await canvas2DContext.SetStrokeStyleAsync("#555");
        await canvas2DContext.SetLineCapAsync(LineCap.Butt);
        await canvas2DContext.BeginPathAsync();
        await canvas2DContext.MoveToAsync(linkSourcePoint.X * aspect, linkSourcePoint.Y * aspect);
        await canvas2DContext.LineToAsync(linkTargetPoint.X * aspect, linkTargetPoint.Y * aspect);
        await canvas2DContext.StrokeAsync();

    }

    public override ValueTask<bool> CheckHighlight(double mousePosX, double mousePosY)
    {
        return ValueTask.FromResult<bool>(mousePosX >= X && mousePosX < X + Width && mousePosY >= Y &&
                                          mousePosY < Y + Height);
    }

    public override ValueTask Drag(double x, double y)
    {
        return ValueTask.CompletedTask;
    }
}