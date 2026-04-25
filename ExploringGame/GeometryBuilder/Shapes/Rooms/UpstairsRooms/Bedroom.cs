using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Services;
using ExploringGame.Texture;
using ExploringGame.GeometryBuilder.Shapes.Appliances;
using Microsoft.Xna.Framework;
using ExploringGame.GeometryBuilder.Shapes.Structures;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;

public class Bedroom : Room
{
    private UpstairsHall _upstairsHall;

    public Bedroom(WorldSegment worldSegment, UpstairsHall upstairsHall) 
        : base(worldSegment, width: Measure.Feet(17 ), depth: Measure.Feet(12), height: Measure.Feet(7))
    {
        _upstairsHall = upstairsHall;
        this.Place().OnSideInner(Side.SouthWest);
    }

    public void LoadChildren()
    {
        _upstairsHall.SouthHall.AddConnectingRoomWithJunction(
            new DoorJunction(this, Side.South, HAlign.Left, DoorDirection.Push,  StateKey.BedroomDoorOpen), this, Side.South, HAlign.Right, 
                adjustPlacement: false);
       
        var light = new HighHatLight(this, 0f, 0f);
        var sw = new LightSwitch(this, Side.East, StateKey.BedroomLightOn);
        sw.ControlledObjects.Add(light);
        sw.Position = this.Position;
        sw.Place().OnSideInner(Side.East).AtEyeLevel(this, -Measure.Inches(5));

        var closet = Copy(width: Measure.Feet(4), depth: Measure.Feet(4));
        closet.Place().OnSideOuter(Side.North, this, -Measure.Inches(6))
            .OnSideInner(Side.West, this, 0.5f);
        closet.AddConnectingRoomWithJunction(new DoorJunction(closet, Side.South, HAlign.Center, DoorDirection.Push, StateKey.BedroomClosetDoorOpen),
            this, Side.South, HAlign.Center, adjustPlacement: false);
    }

    public override Theme Theme =>  new UpstairsHallTheme();
}
