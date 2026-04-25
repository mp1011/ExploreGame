using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Services;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;

public class KidsBedroom : Room
{
    private readonly UpstairsHall _upstairsHall;

    public KidsBedroom(WorldSegment worldSegment, UpstairsHall upstairsHall, Bedroom bedroom)
        : base(worldSegment, width: Measure.Feet(12), depth: Measure.Feet(12), height: Measure.Feet(7))
    {
        _upstairsHall = upstairsHall;
        this.Place().OnSideInner(Side.SouthWest)
            .OnSideOuter(Side.East, bedroom, 0.25f);

    }

    public void LoadChildren()
    {
        SetSideUnanchored(Side.East, GetSide(Side.East) + 0.5f);

        _upstairsHall.SouthHall.AddConnectingRoomWithJunction(
            new DoorJunction(this, Side.South, HAlign.Left, DoorDirection.Push, StateKey.KidsBedroomDoorOpen), this, Side.South, HAlign.Left,
            adjustPlacement: false);

        // Add a high hat light to the kids bedroom
        var kidsLight = new HighHatLight(this, 0f, 0f);

        // Add a light switch on the east wall
        var lightSwitch = new LightSwitch(this, Side.North, StateKey.KidsBedroomLightOn);
        lightSwitch.ControlledObjects.Add(kidsLight);
        lightSwitch.Position = Position;
        lightSwitch.Place().OnSideInner(Side.North)
            .OnSideInner(Side.West, this, 1.5f)
            .AtEyeLevel(this, -Measure.Inches(5));

        var closet = Copy(width: Measure.Feet(4), depth: Measure.Feet(1.4f));
        closet.Place().OnSideOuter(Side.North, this, -Measure.Inches(6))
            .OnSideInner(Side.West, this, 1.7f);
        closet.AddConnectingRoomWithJunction(new DoorJunction(closet, Side.South, HAlign.Center, DoorDirection.Push, StateKey.KidsBedroomClosetDoorOpen),
            this, Side.South, HAlign.Center, adjustPlacement: false);
    }

    public override Theme Theme =>  new UpstairsHallTheme();
}
