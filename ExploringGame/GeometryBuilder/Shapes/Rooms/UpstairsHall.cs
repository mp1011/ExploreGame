using ExploringGame.Config;
using ExploringGame.GeometryBuilder.Shapes.Appliances;
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

    public Room SouthHall { get; private set; }
    public Room NorthHall { get; private set; }

    public UpstairsHall(WorldSegment worldSegment) : base(worldSegment)
    {
        
    }

    public override void LoadChildren()
    {
        SouthHall = Copy(width: Measure.Feet(7), depth: Measure.Feet(10));
        AddConnectingRoom(new RoomConnection(this, SouthHall, Side.South, HAlign.Right));

        NorthHall = Copy(width: Measure.Feet(4), depth: Measure.Feet(14));
        AddConnectingRoom(new RoomConnection(this, NorthHall, Side.North, HAlign.Left));

        var linenCloset = Copy(width: Measure.Feet(1), depth: Door.StandardWidth);
        SouthHall.AddConnectingRoomWithJunction(new DoorJunction(SouthHall, Side.West, HAlign.Left, DoorDirection.Pull, StateKey.LinenClosetDoorOpen),
            other: linenCloset, Side.West, HAlign.Left, offset: 0.2f);

        var hallLight = new HighHatLight(SouthHall, 0f, 0f);

        var hallLightSwitch = new LightSwitch(this, StateKey.HallLightOn);
        hallLightSwitch.ControlledObjects.Add(hallLight);

        hallLightSwitch.Position = Position;
        hallLightSwitch.Place().OnSideInner(Side.West);
    }
}
