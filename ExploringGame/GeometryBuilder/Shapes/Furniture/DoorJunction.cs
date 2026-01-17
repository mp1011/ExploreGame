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

    public DoorJunction(Room room, Side wallSide, HAlign hingePosition, StateKey doorStateKey) : base(room.WorldSegment)
    {
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

        Angle doorOpen, doorClose;

        if(hingePosition == HAlign.Left)
        {
            doorClose = new Angle(wallSide).RotateClockwise(90);
            doorOpen = doorClose.RotateClockwise(90);
        }
        else
        {
            doorClose = new Angle(wallSide).RotateCounterClockwise(90);
            doorOpen = doorClose.RotateCounterClockwise(90);
        }

        _door = new Door(this, doorClose, doorOpen, hingePosition, doorStateKey);
        MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);
    }

    public DoorJunction(WorldSegment worldSegment, Angle doorClose, Angle doorOpen, HAlign hingePosition, StateKey doorStateKey, float width, float height, float depth)
        : base(worldSegment)
    {
        Width = width;
        Height = height;
        Depth = depth;
        _door = new Door(this, doorClose, doorOpen, hingePosition, doorStateKey);
        MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);
    }

    protected override void BeforeBuild()
    {
        _door.Position = Position;

        // todo, this isnt right
        if(_door.ClosedAngle.Degrees == 0f)
        {
            _door.X = GetSide(Side.East) + _door.Width / 2f;
            _door.Z += (_door.Width / 2f);
        }
        else if(_door.ClosedAngle.Degrees == 90f)
        {
            _door.X += _door.Width;
        }
        else if (_door.ClosedAngle.Degrees == 270f)
        {
            _door.X -= _door.Width;
        }
    }
}
