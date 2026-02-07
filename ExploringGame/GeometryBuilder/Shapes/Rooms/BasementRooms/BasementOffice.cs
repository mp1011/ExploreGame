using ExploringGame.Entities;
using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;

public class BasementOffice : Room
{
    public override Theme Theme => new BasementRoomTheme();

    public Room Exit { get; private set; }

    public HighHatLight[] Lights { get; private set; }

    public BasementOffice(WorldSegment worldSegment) : base(worldSegment)
    {
        Width = Measure.Feet(17) - 2.0f;
        Height = Measure.Feet(7);
        Depth = Measure.Feet(22) - 1.9f;

        SetSide(Side.North, worldSegment.GetSide(Side.North) + 1.9f);
        SetSide(Side.Bottom, 0f);

        this.Place().OnSideInner(Side.NorthEast);
    }

    public override void LoadChildren()
    {
        var westPart = Copy(depth: Depth + 1.0f, width: 2.0f);
        AddConnectingRoom(new RoomConnection(this, westPart, Side.West, Align: HAlign.Left));
        westPart.VertexOffsets.Add(new VertexOffset(Side.NorthEast, new Vector3(0f, 0f, 1.0f)));

        Exit = Copy(depth: Measure.Inches(39), width: Measure.Inches(50));
        westPart.AddConnectingRoom(new RoomConnection(westPart, Exit, Side.West, Align: HAlign.Right));

        var eastPart = Copy(width: 2.0f);
        AddConnectingRoom(new RoomConnection(this, eastPart, Side.East, 0.5f));

        var eastPart2 = Copy(depth: 2.0f, width: 2.0f);
        eastPart.AddConnectingRoom(new RoomConnection(eastPart, eastPart2, Side.North, 0.5f));

        var closet1 = new BasementCloset(westPart, Side.East);
        closet1.Place().OnFloor().OnSideInner(Side.SouthWest);

        var closet2 = new BasementCloset(eastPart, Side.West);
        closet2.Place().OnFloor().OnSideInner(Side.SouthEast);

        var ceilingBar = AddChild(new Box(TextureKey.Ceiling));
        ceilingBar.AdjustShape().From(this).SliceFromTop(0.1f, 0.4f).SliceFromNorth(4.0f, 0.4f);
        ceilingBar.SetSideUnanchored(Side.East, eastPart.GetSide(Side.East));
        ceilingBar.SetSideUnanchored(Side.West, westPart.GetSide(Side.West));

        var oilTankRoom = new OilTankRoom(WorldSegment);
        oilTankRoom.Height = Height;
        oilTankRoom.Width = Width - 2.5f;
        oilTankRoom.Depth = 1.9f;

        eastPart2.AddConnectingRoomWithJunction(
            new DoorJunction(oilTankRoom, Side.West, HAlign.Left, DoorDirection.Pull, StateKey.OfficeDoor3Open),
            oilTankRoom, Side.West);

        var desk1 = new OfficeDesk(westPart);
        desk1.Place().OnFloor().OnSideInner(Side.West);
        desk1.Rotation = Rotation.YawFromDegrees(-90);
        desk1.X -= 0.9f;
        desk1.Z += 0.65f;

        var desk2 = new OfficeDesk(eastPart);
        desk2.Place().OnFloor().OnSideInner(Side.East);
        desk2.X += 0.9f;
        desk2.Z += 0.65f;
        desk2.Rotation = Rotation.YawFromDegrees(90);

        var fireplace = new ElectricFireplace(this);
        fireplace.Place().OnFloor().OnSideInner(Side.North);

        var couch = new Couch(this);
        couch.Place().OnFloor().OnSideInner(Side.South);

        Lights =
        [
            new HighHatLight(this, 0f, -0.7f),
            new HighHatLight(this, 3f, -0.7f),

        ];

        // Add test entity for testing
        var testEntity = new TestEntity();
        testEntity.Position = new Vector3(Position.X, Position.Y, Position.Z);
        testEntity.Place().OnFloor(this);
        testEntity.Y += 2.0f;
        AddChild(testEntity);
    }
}
