using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.Motion;
using ExploringGame.Services;
using Jitter2.Dynamics;
using Jitter2.LinearMath;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.Logics;

public class EntityMover : IActiveObject
{
    private readonly Physics _physics;

    public AcceleratedMotion Motion { get; }
    private IWithPosition _entity;
    private RigidBody _body;

    public EntityMover(IWithPosition entity, Physics physics)
    {
        Motion = new AcceleratedMotion();
        _entity = entity;
        _physics = physics;
    }

    public void Initialize()
    {
        _body = _physics.CreateSphere(_entity);
        _body.AffectedByGravity = true;
        _body.Friction = 0;
        _body.Damping = new(0f, 0f);
        _body.SetMassInertia(1.0f);
    }

    public void Update(GameTime gameTime)
    {
        Motion.Update();

        var vy = _body.Velocity.Y;
        _body.Velocity = new JVector(Motion.CurrentMotion.X, vy, Motion.CurrentMotion.Z);
        _entity.Position = _body.Position.ToVector3();
    }

    public void ApplyForce(Vector3 force)
    {
        _body.AddForce(force.ToJVector());
    }
}
