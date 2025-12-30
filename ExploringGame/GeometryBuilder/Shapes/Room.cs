using ExploringGame.Services;
using System.Collections.Generic;

namespace ExploringGame.GeometryBuilder.Shapes;

public class Room : Shape
{
    private List<RoomConnection> _roomConnections = new List<RoomConnection>();

    public override bool CollisionEnabled => true;
    public override ViewFrom ViewFrom => ViewFrom.Inside;

    public void AddConnectingRoom(Room other, Side side)
    {
        var connection = new RoomConnection(other, side);
        _roomConnections.Add(connection);
        other._roomConnections.Add(new RoomConnection(this, side.Opposite()));

        other.Place().OnSideOuter(side, this)
                     .OnSideInner(Side.Bottom, this);

        AddChild(new RoomJoiner(this, other, connection));
        other.AddChild(new RoomJoiner(this, other, connection));
    }


    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        var shape = BuildCuboid();

        foreach(var connection in _roomConnections)
            shape = new RemoveSurfaceRegion().Execute(shape, connection.Side, 
                connection.CalcCutoutPlacement(this));
                
        return shape;
    }

}

public record RoomConnection(Room Other, Side Side)
{
    public Placement2D CalcCutoutPlacement(Room room)
    {
        float left, top, right, bottom;

        top = 0;
        bottom = 0; // fix this when we add varying floor/ceiling heights

        switch(Side)
        {
            case Side.South:
                left = Other.GetSide(Side.West) - room.GetSide(Side.West);
                right = room.GetSide(Side.East) - Other.GetSide(Side.East);
                break;
            case Side.North:
                right = Other.GetSide(Side.West) - room.GetSide(Side.West);
                left = room.GetSide(Side.East) - Other.GetSide(Side.East);
                break;
            case Side.West:
                left = Other.GetSide(Side.North) - room.GetSide(Side.North);
                right = room.GetSide(Side.South) - Other.GetSide(Side.South);
                break;
            case Side.East:
                right = Other.GetSide(Side.North) - room.GetSide(Side.North);
                left = room.GetSide(Side.South) - Other.GetSide(Side.South);
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
