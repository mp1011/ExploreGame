using ExploringGame.Logics.Collision;
using ExploringGame.Services;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes;

public class Room : Shape
{
    private List<RoomConnection> _roomConnections = new List<RoomConnection>();

    public override IColliderMaker ColliderMaker => ColliderMakers.Room(this);

    public override ViewFrom ViewFrom => ViewFrom.Inside;

    public void AddConnectingRoom(RoomConnection connection)
    {
        _roomConnections.Add(connection);
        connection.Other._roomConnections.Add(connection.Reverse(this));

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
                connection.CalcCutoutPlacement(this));
                
        return shape;
    }

}

public record RoomConnection(Room Other, Side Side, float Position)
{
    public RoomConnection Reverse(Room room) => new RoomConnection(room, Side.Opposite(), 1.0f - Position);

    public Placement2D CalcCutoutPlacement(Room room)
    {
        float left, top, right, bottom;

        var thisFloor = room.GetSide(Side.Bottom);
        var otherFloor = Other.GetSide(Side.Bottom);

        var thisCeiling = room.GetSide(Side.Top);
        var otherCeiling = Other.GetSide(Side.Top);

        top = thisCeiling - otherCeiling;
        bottom = otherFloor - thisFloor;
        if (bottom < 0)
            bottom = 0;

        switch(Side)
        {
            case Side.South:
                left = room.GetSide(Side.East) - Other.GetSide(Side.East);
                right = Other.GetSide(Side.West) - room.GetSide(Side.West);
                break;
            case Side.North:
                left = Other.GetSide(Side.West) - room.GetSide(Side.West);
                right = room.GetSide(Side.East) - Other.GetSide(Side.East);
                break;
            case Side.West:
                left = room.GetSide(Side.South) - Other.GetSide(Side.South);
                right = Other.GetSide(Side.North) - room.GetSide(Side.North);
                break;
            case Side.East:
                left = Other.GetSide(Side.North) - room.GetSide(Side.North);
                right = room.GetSide(Side.South) - Other.GetSide(Side.South);
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
