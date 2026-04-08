using ExploringGame.Logics.Collision;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Structures;

public class FencePost : Shape, ICollidable
{
    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public CollisionGroup CollisionGroup => CollisionGroup.Environment;

    public CollisionGroup CollidesWithGroups => CollisionGroup.MovingObjects;

    public override Theme Theme => new Theme(TextureSheetKey.Outdoors, TextureKey.Plain, Color.White);

    public FencePost(Room parent)
    {
        parent.AddChild(this);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }
}
