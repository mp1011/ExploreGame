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
    public const string VentTag = "BedroomVent";

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
       
        var closet = Copy(width: Measure.Feet(4), depth: Measure.Feet(4));
        closet.Place().OnSideOuter(Side.North, this, -Measure.Inches(6))
            .OnSideInner(Side.West, this, 1.0f);
        closet.AddConnectingRoomWithJunction(new DoorJunction(closet, Side.South, HAlign.Center, DoorDirection.Push, StateKey.BedroomClosetDoorOpen),
            this, Side.South, HAlign.Center, adjustPlacement: false);


        var bed = AddChild(new Bed());
        bed.Place().At(this).OnFloor()
            .OnSideInner(Side.West, offset: 0.2f);
        bed.Rotation = Rotation.YawFromDegrees(90);

        var leftEndTable = AddChild(new EndTable());
        leftEndTable.Place().At(this).OnFloor()
            .OnSideInner(Side.West, offset: 0.2f)
            .OnSideInner(Side.South, offset: -0.4f);
        leftEndTable.Rotation = Rotation.YawFromDegrees(90);

        var rightEndTable = AddChild(new EndTable());
        rightEndTable.Place().At(this).OnFloor()
            .OnSideInner(Side.West, offset: 0.2f)
            .OnSideInner(Side.North, offset: 0.4f);
        rightEndTable.Rotation = Rotation.YawFromDegrees(90);

        var leftLamp = leftEndTable.AddChild(new Lamp(this, StateKey.LeftBedroomLightOn));
        leftLamp.Place()
            .AtParent()
            .OnSideOuter(Side.Top);

        rightEndTable.AddChild(new Lamp(this, StateKey.RightBedroomLightOn))
            .Place()
            .AtParent()
            .OnSideOuter(Side.Top);

        var vent = new CeilingVent(this, -2.0f, 1.0f);
        vent.Tag = VentTag;

    }

    public override Theme Theme =>  new UpstairsHallTheme();
}
