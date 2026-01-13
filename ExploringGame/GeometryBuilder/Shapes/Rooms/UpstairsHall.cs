using ExploringGame.Config;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms;

public class UpstairsHall : Room
{
    public override Theme Theme => new UpstairsHallTheme();
    public UpstairsHall(WorldSegment worldSegment) : base(worldSegment)
    {
    }

    public UpstairsHall(WorldSegment worldSegment, TransitionShapesRegistrar transitionShapesRegistrar) 
        : base(worldSegment)
    {
        transitionShapesRegistrar.RecallPositionAndSize(this);
    }
}
