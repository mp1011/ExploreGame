using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using ExploringGame.Texture;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes;

public class Room : Shape
{

    private Theme _theme = Theme.Missing;
    public override Theme Theme => _theme;

    private List<RoomConnection> _roomConnections = new List<RoomConnection>();

    public override IColliderMaker ColliderMaker => ColliderMakers.Room(this);

    public override ViewFrom ViewFrom => ViewFrom.Inside;

    public void AddConnectingRoom(RoomConnection connection)
    {
        _roomConnections.Add(connection);
        connection.Other._roomConnections.Add(connection.Reverse());

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

    public RoomConnection[] GetRoomConnections(Side side) => _roomConnections.Where(p => p.Side == side).ToArray();

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        var shape = BuildCuboid();

        foreach(var connection in _roomConnections)
            shape = new RemoveSurfaceRegion().Execute(shape, connection.Side, 
                connection.CalcCutoutPlacement(), ViewFrom);
                
        return shape;
    }

    public Room Copy(float? height = null, float? width = null, float? depth = null)
    {
        var room = new Room();
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
    public RoomConnection(Room Room, Room Other, Side Side, Side Align) 
        : this(Room, Other, Side, CalcPosition(Room, Other, Align)) { }
    public RoomConnection Reverse() => new RoomConnection(Other, Room, Side.Opposite(), 1.0f - Position);

    private static float CalcPosition(Room room, Room other, Side align)
    {
        throw new System.NotImplementedException();
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
