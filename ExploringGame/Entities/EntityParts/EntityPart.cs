using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Logics;
using ExploringGame.Logics.Collision;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Logics.Controllers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.Entities.EntityParts;


public abstract class EntityPart<TEntity> : PlaceableShape, ICollidable
    where TEntity : PlaceableShape
{
    public TEntity Entity { get; }

    public override CollisionGroup CollisionGroup => CollisionGroup.MovingObjects;

    public override CollisionGroup CollidesWithGroups => CollisionGroup.Environment;

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public override Theme Theme { get; } = new Theme(Color.LightBlue);

    public override IColliderMaker ColliderMaker => ColliderMakers.BoundingBox(this, isStatic: false);

    public EntityPart(TEntity entity)
    {
        Entity = entity;
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }
}