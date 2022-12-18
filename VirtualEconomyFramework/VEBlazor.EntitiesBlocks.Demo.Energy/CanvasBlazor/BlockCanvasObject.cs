using Blazor.Extensions.Canvas.Canvas2D;

namespace VEBlazor.EntitiesBlocks.Demo.Energy.CanvasBlazor;

public class BlockCanvasObject
    : DraggableCanvasObject
{
    public override async ValueTask Render(Canvas2DContext canvas2DContext, bool isHighlighted, float aspect)
    {
        CalculateDragPosition(out var x, out var y);

            
        await canvas2DContext.SetFillStyleAsync("#FF8080");
        await canvas2DContext.SetFillStyleAsync("#FF8080");
        await canvas2DContext.FillRectAsync(x*aspect, y*aspect, Width*aspect, Height*aspect);
        if (isHighlighted)
        {
            await canvas2DContext.SetLineWidthAsync(4*aspect);
            await canvas2DContext.SetStrokeStyleAsync("#000");
            await canvas2DContext.StrokeRectAsync(x*aspect, y*aspect, Width*aspect, Height*aspect);
        }
    }

    public override ValueTask<bool> CheckHighlight(double mousePosX, double mousePosY)
    {
        return ValueTask.FromResult<bool>(mousePosX>=X && mousePosX<X+Width && mousePosY>=Y && mousePosY<Y+Height);
    }

    public override ValueTask Drag(double x, double y)
    {
        dragCurrentX = x;
        dragCurrentY = y;
        return ValueTask.CompletedTask;
    }
}