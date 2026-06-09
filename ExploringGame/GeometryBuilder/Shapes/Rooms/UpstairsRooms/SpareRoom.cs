using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Services;
using ExploringGame.Texture;
using ExploringGame.GeometryBuilder.Shapes.Appliances;
using Microsoft.Xna.Framework;
using ExploringGame.GeometryBuilder.Shapes.Structures;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;

public class SpareRoom : Room
{
    private UpstairsHall _upstairsHall;

    public SpareRoom(WorldSegment worldSegment, UpstairsHall upstairsHall, Bedroom bedroom) 
        : base(worldSegment, height: upstairsHall.Height, width: Measure.Feet(12), depth: Measure.Feet(16))
    {
        _upstairsHall = upstairsHall;
        this.Place().OnSideInner(Side.West);
    }

    public void LoadChildren()
    {
        _upstairsHall.SouthHall.AddConnectingRoomWithJunction(
            new DoorJunction(_upstairsHall.SouthHall, Side.West, HAlign.Left, DoorDirection.Pull, StateKey.SpareRoomDoorOpen),        
            this, Side.West, HAlign.Left, 3.0f, adjustPlacement: false);

        var light = new HighHatLight(this, 0f, 0f);
        var sw = new LightSwitch(this, Side.East, StateKey.SpareRoomLightOn);
        sw.ControlledObjects.Add(light);
        sw.LocalPosition = this.LocalPosition;
        sw.Place().OnSideInner(Side.East).AtEyeLevel(this, -Measure.Inches(5));

        var closet = Copy(width: Measure.Feet(4), depth: Measure.Feet(4));
        closet.Place().OnSideOuter(Side.South, this, Measure.Inches(6))
            .OnSideInner(Side.East, this, -0.5f);
        closet.AddConnectingRoomWithJunction(new DoorJunction(closet, Side.North, HAlign.Center, DoorDirection.Push, StateKey.SpareRoomClosetDoorOpen),
            this, Side.North, HAlign.Center, adjustPlacement: false);
    }

    public override Theme Theme => new UpstairsHallTheme();
}
