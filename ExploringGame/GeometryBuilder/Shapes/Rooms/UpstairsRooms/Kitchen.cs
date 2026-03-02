using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Services;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;

public class Kitchen : Room
{
    private readonly UpstairsHall _upstairsHall;

    public override Theme Theme => new KitchenTheme();
    public Kitchen(WorldSegment worldSegment, UpstairsHall upstairsHall) 
        : base(worldSegment, height: upstairsHall.Height, width: 4f, depth: 4f)
    {
        _upstairsHall = upstairsHall;
    }

    public override void LoadChildren()
    {
        _upstairsHall.AddConnectingRoom(new RoomConnection(_upstairsHall, this, Side.East, HAlign.Right));
        SetSideUnanchored(Side.North, _upstairsHall.NorthHall.GetSide(Side.North));

        // Place window on east wall, align right, 2 feet from wall
        var windowEast = new Window(this, Side.East, Measure.Feet(4), Measure.Feet(4), HAlign.Right, -Measure.Feet(2));

        var light = new HighHatLight(this, 0f, 0f);
        var lightSwitch = new LightSwitch(this, Side.West, StateKey.KitchenLightOn);
        lightSwitch.ControlledObjects.Add(light);

        lightSwitch.Position = Position;
        lightSwitch.Place().OnSideInner(Side.West);
    }
}
