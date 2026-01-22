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

    public DoubleDoorJunction(Room room, Side wallSide, StateKey doorStateKey) : base(room.WorldSegment)
    {
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

        var doorClose = new Angle(wallSide).RotateClockwise(90);
        var doorOpen = doorClose.RotateClockwise(90);
        _leftDoor = new Door(this, doorClose, doorOpen, HAlign.Left, doorStateKey);

        doorClose = new Angle(wallSide).RotateCounterClockwise(90);
        doorOpen = doorClose.RotateCounterClockwise(90);
        _rightDoor = new Door(this, doorClose, doorOpen, HAlign.Right, doorStateKey);
             
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
        // todo, this isnt right
        if (door.ClosedAngle.Degrees == 0f)
        {
            door.X -= 0.6f;
            door.Z += door.Width;
        }
        else if (door.ClosedAngle.Degrees == 180f)
        {
            door.X += 0.6f;
            door.Z -= door.Width;
        }
    }
}
