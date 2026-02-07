using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Services;

namespace ExploringGame.Testing;

public class TestStampedShape : StampedShape<TestShapeStamp>
{
    public override CollisionGroup CollisionGroup => CollisionGroup.None;
    public override CollisionGroup CollidesWithGroups => CollisionGroup.None;

    public override ViewFrom ViewFrom => ViewFrom.Outside;
}
