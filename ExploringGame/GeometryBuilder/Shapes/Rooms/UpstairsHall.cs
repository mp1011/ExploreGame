using ExploringGame.Config;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System.ComponentModel;

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

    public override void LoadChildren()
    {
        if (SouthHall == null)
            return;

        var linenCloset = Copy(width: Measure.Feet(1), depth: Door.StandardWidth);
        SouthHall.AddConnectingRoomWithJunction(new DoorJunction(SouthHall, Side.West, HAlign.Left, StateKey.LinenClosetDoorOpen),
            other: linenCloset, Side.West, HAlign.Left, offset: 0.2f);
    }
}
