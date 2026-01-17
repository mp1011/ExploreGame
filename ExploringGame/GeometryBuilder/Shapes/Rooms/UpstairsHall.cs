using ExploringGame.Config;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Services;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms;

public class UpstairsHall : Room
{
    public override Theme Theme => new UpstairsHallTheme();

    public Room SouthHall { get; }

    public UpstairsHall(WorldSegment worldSegment) : base(worldSegment)
    {
        
    }

    public UpstairsHall(WorldSegment worldSegment, TransitionShapesRegistrar transitionShapesRegistrar) 
        : base(worldSegment)
    {
        transitionShapesRegistrar.RecallPositionAndSize(this);

        SouthHall = Copy(width: Measure.Feet(7), depth: Measure.Feet(10));
        AddConnectingRoom(new RoomConnection(this, SouthHall, Side.South, HAlign.Right));


        var northHall = Copy(width: Measure.Feet(4), depth: Measure.Feet(10));
        AddConnectingRoom(new RoomConnection(this, northHall, Side.North, HAlign.Left));
    }
}
