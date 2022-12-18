using Blazor.Extensions.Canvas.Canvas2D;
using Microsoft.AspNetCore.Components;

namespace VEBlazor.EntitiesBlocks.Demo.Energy.CanvasBlazor;

public class LinkCanvasObject
    : DraggableCanvasObject
{
    

    public LinkCanvasObject(double x, double y, double width, double height, ElementReference image)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Image = image;
    }

    public override async ValueTask Render(Canvas2DContext canvas2DContext, bool isHighlighted, float aspect)
    {
        CalculateDragPosition(out var x, out var y);

        await canvas2DContext.SetLineDashOffsetAsync(5 * aspect);
        await canvas2DContext.SetLineWidthAsync(2);
        await canvas2DContext.SetStrokeStyleAsync("#888");
        await canvas2DContext.BeginPathAsync();
        await canvas2DContext.MoveToAsync(X * aspect + Width * aspect / 2, Y * aspect + Height * aspect / 2);
        await canvas2DContext.LineToAsync(x * aspect + Width * aspect / 2, y * aspect + Height * aspect / 2);
        await canvas2DContext.StrokeAsync();
        await canvas2DContext.DrawImageAsync(Image, x * aspect, y * aspect, Width * aspect, Height * aspect);

        if (isHighlighted)
        {
            await canvas2DContext.SetFillStyleAsync("#808000");
            await canvas2DContext.FillRectAsync(X * aspect + 15 * aspect, Y * aspect - 35 * aspect, 75 * aspect,
                35 * aspect);
            await canvas2DContext.SetStrokeStyleAsync("#000");
            await canvas2DContext.SetLineWidthAsync(0.5f);
            await canvas2DContext.SetLineJoinAsync(LineJoin.Round);

            await canvas2DContext.StrokeRectAsync(X * aspect + 15 * aspect, Y * aspect - 35 * aspect, 75 * aspect,
                35 * aspect);
            await canvas2DContext.SetFillStyleAsync("#FFF");
            await canvas2DContext.SetFontAsync("12px consolas");
            await canvas2DContext.FillTextAsync($"Drag me", X * aspect + 20 * aspect, Y * aspect - 15 * aspect);

        }
    }

    public override ValueTask<bool> CheckHighlight(double mousePosX, double mousePosY)
    {
        return ValueTask.FromResult<bool>(mousePosX >= X && mousePosX < X + Width && mousePosY >= Y &&
                                          mousePosY < Y + Height);
    }

    public override ValueTask Drag(double x, double y)
    {
        dragCurrentX = x;
        dragCurrentY = y;
        return ValueTask.CompletedTask;
    }

    public ElementReference Image { get; set; }
}
    