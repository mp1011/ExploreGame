using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Furniture;

/// <summary>
/// double door junction
/// </summary>
public class DoubleDoorJunction : Room
{
    private Door _leftDoor, _rightDoor;
    private Side _wallSide;

    public DoubleDoorJunction(Room room, Side wallSide, DoorDirection doorDirection, StateKey doorStateKey) : base(room.WorldSegment)
    {
        _wallSide = wallSide;
        if(wallSide.GetAxis() == Axis.Z)
        {
            Width = Door.StandardWidth * 2;
            Depth = 0.2f;
            Height = room.Height;
        }
        else
        {
            Depth = Door.StandardWidth * 2;
            Width = 0.2f;
            Height = room.Height;
        }

        _leftDoor = new Door(this, wallSide, HAlign.Left, doorDirection, doorStateKey);
        _rightDoor = new Door(this, wallSide, HAlign.Right, doorDirection, doorStateKey);
             
        MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);
    }


    protected override void BeforeBuild()
    {
        _leftDoor.Position = Position;
        _rightDoor.Position = Position;

        PlaceDoor(_leftDoor);
        PlaceDoor(_rightDoor);
    }

    private void PlaceDoor(Door door)
    {
        door.Position = Position;

        var hingeSide = door.HingePosition == HAlign.Left ? _wallSide.CounterClockwiseTurn()
                                                           : _wallSide.ClockwiseTurn();

        var hingePosition = Position.SetAxis(hingeSide.GetAxis(), GetSide(hingeSide));
        door.SetHingePosition(hingePosition);
    }
}
