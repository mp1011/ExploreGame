using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Furniture;

/// <summary>
/// mini "room" which joins two other rooms with a door
/// </summary>
public class DoorJunction : Room
{
    private Door _door;

    public DoorJunction(Angle doorClose, Angle doorOpen, HingePosition hingePosition, float height)
    {
        Height = height;
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
