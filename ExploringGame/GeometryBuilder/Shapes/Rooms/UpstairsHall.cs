using ExploringGame.Config;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Services;
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

        var southHall = Copy(width: Measure.Feet(7), depth: Measure.Feet(10));
        AddConnectingRoom(new RoomConnection(this, southHall, Side.South, HAlign.Right));


        var northHall = Copy(width: Measure.Feet(4), depth: Measure.Feet(10));
        AddConnectingRoom(new RoomConnection(this, northHall, Side.North, HAlign.Left));


    }
}
