using Blazor.Extensions.Canvas.Canvas2D;

namespace VEBlazor.EntitiesBlocks.Demo.Energy.CanvasBlazor;

public class AreaCanvasObject
    : DraggableCanvasObject
{
    public string Caption { get; set; }

    public override async ValueTask Render(Canvas2DContext canvas2DContext, bool isHighlighted, float aspect)
    {
        CalculateDragPosition(out var x, out var y);

            
        await canvas2DContext.SetFillStyleAsync("#FF808080");
        await canvas2DContext.SetLineDashAsync(new[] {5f, 5f});
        await canvas2DContext.FillRectAsync(x*aspect, y*aspect, Width*aspect, Height*aspect);
        if (isHighlighted)
        {
            await canvas2DContext.SetLineWidthAsync(4*aspect);
            await canvas2DContext.SetStrokeStyleAsync("#000");
            await canvas2DContext.SetStrokeStyleAsync("#000");
            await canvas2DContext.SetLineDashAsync(new[] {5f, 5f});
            await canvas2DContext.StrokeRectAsync(x*aspect, y*aspect, Width*aspect, Height*aspect);
        }



        if (!string.IsNullOrWhiteSpace(Caption))
        {
            await canvas2DContext.SetFillStyleAsync("#FFF");
            var size = (int)Math.Round(12 * aspect, MidpointRounding.AwayFromZero);
            await canvas2DContext.SetFontAsync($"{size}px consolas");
            var captionMetrics = await canvas2DContext.MeasureTextAsync(Caption);
            await canvas2DContext.FillTextAsync(Caption, x * aspect + aspect * (Width - captionMetrics.Width) / 2,
                y * aspect + 15 * aspect);
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