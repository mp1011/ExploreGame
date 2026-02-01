using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Services;
using ExploringGame.Texture;
using ExploringGame.GeometryBuilder.Shapes.Appliances;
using Microsoft.Xna.Framework;

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

    public override void LoadChildren()
    {
        _upstairsHall.SouthHall.AddConnectingRoomWithJunction(
            new DoorJunction(_upstairsHall.SouthHall, Side.West, HAlign.Left, DoorDirection.Pull, StateKey.SpareRoomDoorOpen),        
            this, Side.West, HAlign.Left, 3.0f, adjustPlacement: false);

        var light = new HighHatLight(this, 0f, 0f);
        var sw = new LightSwitch(this, Side.East, StateKey.SpareRoomLightOn);
        sw.ControlledObjects.Add(light);
        sw.Position = this.Position;
        sw.Place().OnSideInner(Side.East);
    }

    public override Theme Theme => new UpstairsHallTheme();
}
