using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Furniture;

/// <summary>
/// mini "room" which joins two other rooms with a door
/// </summary>
public class DoorJunction : Room
{
    private Door _door;

    public DoorJunction(WorldSegment worldSegment, Angle doorClose, Angle doorOpen, HAlign hingePosition, float width, float height, float depth)
        : base(worldSegment)
    {
        Width = width;
        Height = height;
        Depth = depth;
        _door = new Door(this, doorClose, doorOpen, hingePosition);

        MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);
    }

    protected override void BeforeBuild()
    {
        _door.Position = Position;
        _door.X = GetSide(Side.East) + _door.Width / 2f;
        _door.Z += (_door.Width / 2f);
    }
}
