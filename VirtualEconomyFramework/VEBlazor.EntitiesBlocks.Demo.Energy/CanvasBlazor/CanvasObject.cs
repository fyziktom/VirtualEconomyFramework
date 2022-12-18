using Blazor.Extensions.Canvas.Canvas2D;

namespace VEBlazor.EntitiesBlocks.Demo.Energy.CanvasBlazor;

public abstract class CanvasObject
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public bool IsMouseDown { get; set; }
    public bool IsDragged { get; set; }
    public double DragStartX { get; set; }
    public double DragStartY { get; set; }

    public abstract ValueTask Render(Canvas2DContext canvas2DContext, bool isHighlighted, float aspect);
    public abstract ValueTask<bool> CheckHighlight(double mousePosX, double mousePosY);

    public abstract ValueTask Drag(double x, double y);

    public virtual ValueTask DragStop()
    {
        return ValueTask.CompletedTask;
    }
}