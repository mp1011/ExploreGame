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
    private readonly Room _backyardMid, _backyardSouth;

    public KidsBedroom(WorldSegment worldSegment, UpstairsHall upstairsHall, Bedroom bedroom, Room backyardMid, Room backyardSouth)
        : base(worldSegment, width: Measure.Feet(12), depth: Measure.Feet(12), height: Measure.Feet(7))
    {
        _upstairsHall = upstairsHall;
        _backyardMid = backyardMid;
        _backyardSouth = backyardSouth;
        this.Place().OnSideInner(Side.SouthWest)
            .OnSideOuter(Side.East, bedroom, 0.25f);
         
    }

    public override void LoadChildren()
    {
        SetSideUnanchored(Side.East, GetSide(Side.East) + 0.5f);

        _upstairsHall.SouthHall.AddConnectingRoomWithJunction(
            new DoorJunction(this, Side.South, HAlign.Left, DoorDirection.Push, StateKey.KidsBedroomDoorOpen), this, Side.South, HAlign.Left,
            adjustPlacement: false);

        // Place windows on south and east walls
        var windowSouth = new Window(this, Side.South, Measure.Feet(3), Measure.Feet(4), otherRoom: _backyardSouth);
        var windowEast = new Window(this, Side.East, Measure.Feet(3), Measure.Feet(4), otherRoom: _backyardMid);

        windowSouth.Tag = "KidBedroomSouthWindow";
        windowEast.Tag = "KidBedroomEastWindow";

        // Add a high hat light to the kids bedroom
        var kidsLight = new HighHatLight(this, 0f, 0f);

        // Add a light switch on the east wall
        var lightSwitch = new LightSwitch(this, Side.North, StateKey.KidsBedroomLightOn);
        lightSwitch.ControlledObjects.Add(kidsLight);
        lightSwitch.Position = Position;
        lightSwitch.Place().OnSideInner(Side.North);
    }

    public override Theme Theme =>  new UpstairsHallTheme();
}
