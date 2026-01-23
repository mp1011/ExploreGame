using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Furniture;

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
        _door.Position = Position;

        var hingeSide = _door.HingePosition == HAlign.Left ? _wallSide.CounterClockwiseTurn()
                                                           : _wallSide.ClockwiseTurn();

        var hingePosition = Position.SetAxis(hingeSide.GetAxis(), GetSide(hingeSide));
        _door.SetHingePosition(hingePosition);
    }
}
