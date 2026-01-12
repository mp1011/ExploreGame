using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using ExploringGame.Texture;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes;

public class Room : Shape
{
    protected WorldSegment _worldSegment;
    private Theme _theme = Theme.Missing;
    public override Theme Theme => _theme;

    private List<RoomConnection> _roomConnections = new List<RoomConnection>();

    public override IColliderMaker ColliderMaker => ColliderMakers.Room(this);

    public override ViewFrom ViewFrom => ViewFrom.Inside;
    
    public List<VertexOffset> VertexOffsets { get; } = new();

    protected virtual Side OmitSides { get; }

    public Room(WorldSegment worldSegment, Theme theme = null)
    {
        if (theme != null)
            _theme = theme;
        _worldSegment = worldSegment;
        worldSegment.AddChild(this);
    }

    public virtual void LoadChildren()
    {

    }

    public void AddConnectingRoom(RoomConnection connection, bool adjustPlacement = true)
    {
        if (_roomConnections.Any(p => p.Side == connection.Side))
            throw new System.NotSupportedException("multiple connections on the same side are not yet supported");
        
        if(connection.Room != this)
            throw new System.ArgumentException("connection.Room must be this room");

        _roomConnections.Add(connection);
        connection.Other._roomConnections.Add(connection.Reverse());

        if (adjustPlacement)
        {
            connection.Other.Position = Position;
            connection.Other.Place().OnSideOuter(connection.Side, this)
                                    .OnSideInner(Side.Bottom, this);

            var pos = connection.Position;

            // todo, don't like this
            if (connection.Side == Side.West || connection.Side == Side.South)
                pos = 1.0f - pos;

            var connectionPoint = RelativeAxisPoint(connection.Side.GetAxis().Orthogonal(), pos);
            connection.Other.SetAxisPosition(connection.Side.GetAxis().Orthogonal(), connectionPoint);
        }
    }

    public void AddConnectingRoomWithJunction(DoorJunction doorJunction, Room other, Side side)
    {
        AddConnectingRoom(new RoomConnection(this, doorJunction, side));
        doorJunction.AddConnectingRoom(new RoomConnection(doorJunction, other, side));
    }

    public RoomConnection[] GetRoomConnections(Side side) => _roomConnections.Where(p => p.Side == side).ToArray();

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        var shape = BuildCuboid();
        shape = new SideRemover().Execute(shape, OmitSides);

        foreach(var connection in _roomConnections)
            shape = new RemoveSurfaceRegion().Execute(shape, connection.Side, 
                connection.CalcCutoutPlacement(), ViewFrom);

        shape = new RemoveSurfaceRegion().RemoveCutouts(this, shape);

        foreach(var vertexOffset in VertexOffsets)
            shape = new VertexOffsetter().Execute(this, shape, vertexOffset);

        return shape;
    }

    public Room Copy(float? height = null, float? width = null, float? depth = null)
    {
        var room = new Room(_worldSegment);
        room._theme = Theme;
        room.Position = Position;
        room.Size = Size;

        if(height.HasValue)
            room.Height = height.Value;

        if (width.HasValue)
            room.Width = width.Value;

        if (depth.HasValue)
            room.Depth = depth.Value;

        return room;
    }
}

public record RoomConnection(Room Room, Room Other, Side Side, float Position = 0.5f)
{
    public RoomConnection(Room Room, Room Other, Side Side, HAlign Align, float Offset=0f) 
        : this(Room, Other, Side, CalcPosition(Room, Other, Side, Align, Offset)) { }
    public RoomConnection Reverse() => new RoomConnection(Other, Room, Side.Opposite(), 1.0f - Position);

    private static float CalcPosition(Room room, Room other, Side side, HAlign align, float offset)
    {
        switch(align)
        {
            case HAlign.Center:
                return 0.5f;
            case HAlign.Left:
                return ((other.SideLength(side) / 2f) + offset) / room.SideLength(side);
            case HAlign.Right:
                return (room.SideLength(side) - (other.SideLength(side) / 2f) + offset) / room.SideLength(side);
            default:
                throw new System.ArgumentException("Invalid side");
        }
    }
    
    public Placement2D CalcCutoutPlacement()
    {
        float left, top, right, bottom;

        var thisFloor = Room.GetSide(Side.Bottom);
        var otherFloor = Other.GetSide(Side.Bottom);

        var thisCeiling = Room.GetSide(Side.Top);
        var otherCeiling = Other.GetSide(Side.Top);

        top = thisCeiling - otherCeiling;
        bottom = otherFloor - thisFloor;
        if (bottom < 0)
            bottom = 0;

        switch(Side)
        {
            case Side.South:
                left = Room.GetSide(Side.East) - Other.GetSide(Side.East);
                right = Other.GetSide(Side.West) - Room.GetSide(Side.West);
                break;
            case Side.North:
                left = Other.GetSide(Side.West) - Room.GetSide(Side.West);
                right = Room.GetSide(Side.East) - Other.GetSide(Side.East);
                break;
            case Side.West:
                left = Room.GetSide(Side.South) - Other.GetSide(Side.South);
                right = Other.GetSide(Side.North) - Room.GetSide(Side.North);
                break;
            case Side.East:
                left = Other.GetSide(Side.North) - Room.GetSide(Side.North);
                right = Room.GetSide(Side.South) - Other.GetSide(Side.South);
                break;
            default:
                throw new System.NotImplementedException("fix me");

        }

        if(left < 0)
            left = 0;
        if (right < 0)
            right = 0;

        return new Placement2D(left, top, right, bottom);
    }
}
