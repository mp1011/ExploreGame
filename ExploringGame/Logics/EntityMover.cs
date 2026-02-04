using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using ExploringGame.Logics.Collision;
using ExploringGame.Motion;
using ExploringGame.Services;
using Jitter2.Dynamics;
using Jitter2.LinearMath;
using Microsoft.Xna.Framework;
using System.Linq;

namespace ExploringGame.Logics;

public class EntityMover : IActiveObject
{
    private readonly Physics _physics;

    public AcceleratedMotion Motion { get; }
    private ICollidable _entity;
    private RigidBody _body;
    private bool _initialPositionSet = false;

    public CollisionResponder CollisionResponder { get; }

    public EntityMover(ICollidable entity, Physics physics)
    {
        Motion = new AcceleratedMotion();
        _entity = entity;
        _physics = physics;
        CollisionResponder = new CollisionResponder(this);
    }

    public void Initialize()
    {
        _body = _entity.ColliderBodies.FirstOrDefault();
        CollisionResponder.Subscribe(_body);
        _initialPositionSet = false;
    }

    public void Stop()
    {
        _body = null;
    }

    public void Update(GameTime gameTime)
    {
        if (!_initialPositionSet)
            SetInitialPosition();

        Motion.Update();

        // handling Y separately (also note Jitter2 has up as +Y
        _body.Velocity = new JVector(Motion.CurrentMotion.X, -Motion.CurrentY, Motion.CurrentMotion.Z);

        _entity.Position = _body.Position.ToVector3();

        CollisionResponder.Update();
    }

    private void SetInitialPosition()
    {
        _body.Position = _entity.Position.ToJVector();
        _initialPositionSet = true;
    }
}
