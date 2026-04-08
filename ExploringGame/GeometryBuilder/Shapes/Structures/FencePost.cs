using ExploringGame.Logics.Collision;
using ExploringGame.Services;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Structures;

public class FencePost : Shape, ICollidable
{
    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public CollisionGroup CollisionGroup => CollisionGroup.Environment;

    public CollisionGroup CollidesWithGroups => CollisionGroup.MovingObjects;

    public override Theme Theme => new FenceTheme();

    public FencePost(Room parent)
    {
        parent.AddChild(this);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }
}
