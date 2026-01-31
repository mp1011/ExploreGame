using ExploringGame.GeometryBuilder.Shapes.Appliances;
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

        var light = new HighHatLight(this, 0f, 0f);
        var lightSwitch = new LightSwitch(this, StateKey.KitchenLightOn);
        lightSwitch.ControlledObjects.Add(light);

        lightSwitch.Position = Position;
        lightSwitch.Place().OnSideInner(Side.West);
    }

}
