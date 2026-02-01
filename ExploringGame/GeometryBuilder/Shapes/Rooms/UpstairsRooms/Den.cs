using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Services;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;

public class Den : Room
{
    private LivingRoom _livingRoom;

    public Room EastPart { get; private set;  }
    public override Theme Theme => new UpstairsHallTheme();

    public Den(UpstairsWorldSegment worldSegment, LivingRoom livingRoom) 
        : base(worldSegment, height: livingRoom.Height, width: Measure.Feet(17), depth: Measure.Feet(23))
    {
        _livingRoom = livingRoom;
        this.Place().OnSideInner(Side.NorthEast);
    }

    public override void LoadChildren()
    {
        _livingRoom.AddConnectingRoomWithJunction(new DoubleDoorJunction(this, Side.East, DoorDirection.Push, StateKey.DenDoorsOpen), 
            this, Side.East, HAlign.Right, -1.0f, adjustPlacement: false);
        
        EastPart = Copy(depth: Measure.Feet(5), width: Measure.Feet(5));
        AddConnectingRoom(new RoomConnection(this, EastPart, Side.East, HAlign.Right));

        var closet = Copy(depth: Measure.Feet(5), width: Measure.Feet(5));

        AddConnectingRoomWithJunction(new DoorJunction(closet, Side.East, HAlign.Left, DoorDirection.Pull, StateKey.DenClosetDoorOpen), closet, Side.East, HAlign.Left, offset: 0.5f);

        var light = new HighHatLight(this, 0f, 0f);
        var lightSwitch = new LightSwitch(this, Side.South, StateKey.DenLightOn);
        lightSwitch.ControlledObjects.Add(light);

        lightSwitch.Position = Position;
        lightSwitch.Place().OnSideInner(Side.South);
    }
}
