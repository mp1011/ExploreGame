using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Services;
using ExploringGame.Texture;
using System.Numerics;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;

public class Kitchen : Room
{
    public Window Window { get; private set;  }

    private readonly UpstairsHall _upstairsHall;

    public override Theme Theme => new KitchenTheme();
    public Kitchen(WorldSegment worldSegment, UpstairsHall upstairsHall) 
        : base(worldSegment, height: upstairsHall.Height, width: 4f, depth: 4f)
    {
        _upstairsHall = upstairsHall;
    }

    public void LoadChildren()
    {
        _upstairsHall.AddConnectingRoom(new RoomConnection(_upstairsHall, this, Side.East, HAlign.Right));
        SetSideUnanchored(Side.North, _upstairsHall.NorthHall.GetSide(Side.North));

        // Place window on east wall, align right, 2 feet from wall
       // Window = new Window(this, Side.East, Measure.Feet(4), Measure.Feet(4), HAlign.Right, -Measure.Feet(2), otherRoom: backDeckArea);

        var light = new HighHatLight(this, 0f, 0f);
        var lightSwitch = new LightSwitch(this, Side.West, StateKey.KitchenLightOn);
        lightSwitch.ControlledObjects.Add(light);

        lightSwitch.Position = Position;
        lightSwitch.Place().OnSideInner(Side.West);

        var southPart = Copy();
        southPart.Depth = Measure.Feet(4);
        southPart.Width = Width - Measure.Feet(4);
        southPart.Place().OnSideOuter(Side.South, this)
            .OnSideInner(Side.East, this);

        AddConnectingRoom(southPart, Side.South);

    }
}
