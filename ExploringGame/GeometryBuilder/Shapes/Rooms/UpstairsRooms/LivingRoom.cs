using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;
using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;

public class LivingRoom : Room 
{
    private readonly UpstairsHall _upstairsHall;
    private readonly Kitchen _kitchen;
    public override Theme Theme => new LivingRoomTheme();

    public LivingRoom(WorldSegment segment, UpstairsHall upstairs, Kitchen kitchen) 
        : base(segment, height: Measure.Feet(7), depth: Measure.Feet(17), width: Measure.Feet(24))
    {
        _upstairsHall = upstairs;
        _kitchen = kitchen;

        this.Place().OnSideInner(Side.NorthWest);
    }

    public void LoadChildren(FrontDeck frontDeck)
    {
        AddConnectingRoom(new RoomConnection(this, _kitchen, Side.South), adjustPlacement: false);
        AddConnectingRoom(new RoomConnection(this, _upstairsHall.NorthHall, Side.South), adjustPlacement: false);

       
        var light = new HighHatLight(this, 0f, 0f);
        var lightSwitch = new LightSwitch(this, Side.East, StateKey.LivingRoomLightOn);
        lightSwitch.ControlledObjects.Add(light);

        lightSwitch.LocalPosition = LocalPosition;
        lightSwitch.Place().OnSideInner(Side.East).AtStandardSwitchHeight();

        SetSideUnanchored(Side.North, GetSide(Side.North) - Measure.Feet(5));
    }
}
