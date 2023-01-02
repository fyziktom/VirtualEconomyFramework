namespace VEBlazor.EntitiesBlocks.Demo.Energy.CanvasBlazor;

public interface ILinkableCanvasObject
{
    CanvasPoint GetLinkSourcePoint();
    CanvasPoint GetLinkTargetPoint();
}