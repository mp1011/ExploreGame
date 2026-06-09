using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes.Structures;

/// <summary>
/// mini "room" which joins two other rooms with a door
/// </summary>
public class DoorJunction : Room
{
    private Door _door;
    private Side _wallSide;

    public DoorJunction(Room room, Side wallSide, HAlign hingePosition, DoorDirection doorDirection, StateKey doorStateKey) : base(room.WorldSegment)
    {
        _wallSide = wallSide;
        if(wallSide.GetAxis() == Axis.Z)
        {
            Width = Door.StandardWidth;
            Depth = 0.2f;
            Height = room.Height;
        }
        else
        {
            Depth = Door.StandardWidth;
            Width = 0.2f;
            Height = room.Height;
        }

        _door = new Door(this, wallSide, hingePosition, doorDirection, doorStateKey);
        MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);
    }

    protected override void BeforeBuild()
    {
        _door.LocalPosition = LocalPosition;

        var hingeSide = _door.HingePosition == HAlign.Left ? _wallSide.CounterClockwiseTurn()
                                                           : _wallSide.ClockwiseTurn();

        var hingePosition = LocalPosition.SetAxis(hingeSide.GetAxis(), GetLocalSide(hingeSide));
        _door.SetHingePosition(hingePosition);
    }

    public override string ToString()
    {
        return "Junction: " + string.Join(" - ", RoomConnections.Select(p => p.GetOtherRoom(this).ToString()).ToArray());
    }
}
