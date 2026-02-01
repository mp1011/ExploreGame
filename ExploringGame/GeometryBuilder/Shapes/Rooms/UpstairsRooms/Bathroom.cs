using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Texture;
using ExploringGame.GeometryBuilder.Shapes.Appliances;
using Microsoft.Xna.Framework;
using ExploringGame.Services;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;

public class Bathroom : Room
{
    private readonly UpstairsHall _upstairsHall;
    public override Theme Theme => new BathroomTheme();

    public Bathroom(UpstairsWorldSegment worldSegment, UpstairsHall hall) 
        : base(worldSegment, height: hall.Height, width: Measure.Feet(10), depth: Measure.Feet(5))
    {
        _upstairsHall = hall;
    }

    public override void LoadChildren()
    {
        _upstairsHall.SouthHall.AddConnectingRoomWithJunction(
            new DoorJunction(this, Side.East, HAlign.Right, DoorDirection.Pull, StateKey.BathroomDoorOpen), this, Side.East);

        var light = new HighHatLight(this, 0f, 0f, initialState: false);
        var sw = new LightSwitch(this, Side.North, StateKey.BathroomLightOn);
        sw.ControlledObjects.Add(light);
        sw.Position = this.Position;
        sw.Place().OnSideInner(Side.North);
    }
}
