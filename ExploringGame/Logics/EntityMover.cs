using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.Logics.Collision;
using ExploringGame.Motion;
using ExploringGame.Services;
using Jitter2.Dynamics;
using Jitter2.LinearMath;
using Microsoft.Xna.Framework;

namespace ExploringGame.Logics;

public class EntityMover : IActiveObject
{
    private readonly Physics _physics;

    public AcceleratedMotion Motion { get; }
    private IWithPosition _entity;
    private RigidBody _body;

    public CollisionResponder CollisionResponder { get; }

    public EntityMover(IWithPosition entity, Physics physics)
    {
        Motion = new AcceleratedMotion();
        _entity = entity;
        _physics = physics;
        CollisionResponder = new CollisionResponder(this);
    }

    public void Initialize()
    {
        _body = _physics.CreateSphere(_entity);
        _body.AffectedByGravity = false;
        _body.Friction = 0;
        _body.Damping = new(0f, 0f);
        _body.SetMassInertia(1.0f);
        CollisionResponder.Subscribe(_body);
    }

    public void Update(GameTime gameTime)
    {
        Motion.Update();

        // handling Y separately (also note Jitter2 has up as +Y
        _body.Velocity = new JVector(Motion.CurrentMotion.X, -Motion.CurrentY, Motion.CurrentMotion.Z);

        _entity.Position = _body.Position.ToVector3();

        CollisionResponder.Update();
    }
}
