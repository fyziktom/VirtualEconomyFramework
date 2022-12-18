using Blazor.Extensions.Canvas.Canvas2D;

namespace VEBlazor.EntitiesBlocks.Demo.Energy.CanvasBlazor;

public class DraggableCanvasObject
    : CanvasObject
{
    protected double dragCurrentX;
    protected double dragCurrentY;
    public override ValueTask Render(Canvas2DContext canvas2DContext, bool isHighlighted, float aspect)
    {
        throw new NotImplementedException();
    }

    public override ValueTask<bool> CheckHighlight(double mousePosX, double mousePosY)
    {
        throw new NotImplementedException();
    }

    public override ValueTask Drag(double x, double y)
    {
        throw new NotImplementedException();
    }

    public void CalculateDragPosition(out double x, out double y)
    {
        x = X;
        y = Y;
        if (!IsDragged) return;
        
        var offsetX = DragStartX - X;
        var offsetY = DragStartY - Y;
        x = dragCurrentX - offsetX;
        y = dragCurrentY - offsetY;
    }
}