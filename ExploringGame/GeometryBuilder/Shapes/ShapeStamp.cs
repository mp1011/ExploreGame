using ExploringGame.Logics.Collision.ColliderMakers;

namespace ExploringGame.GeometryBuilder;

/// <summary>
/// A template shape that generates geometry once but can be instantiated multiple times
/// via StampedShape. These are not directly rendered
/// </summary>
public abstract class ShapeStamp : Shape
{
    public override IColliderMaker ColliderMaker => null;
    
    public override ViewFrom ViewFrom => ViewFrom.Outside;
}
