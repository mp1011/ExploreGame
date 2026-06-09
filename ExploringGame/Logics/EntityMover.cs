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
    private readonly bool _ignoreY;

    public AcceleratedMotion Motion { get; }
    private ICollidable _entity;
    private RigidBody _body;
    private bool _initialPositionSet = false;

    public CollisionResponder CollisionResponder { get; }

    public bool Active { get; set; }

    public EntityMover(ICollidable entity, Physics physics, bool ignoreY = true)
    {
        Active = true;
        Motion = new AcceleratedMotion();
        _entity = entity;
        _physics = physics;
        _ignoreY = ignoreY;
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
        if(!Active)
        {
            _body.Velocity = JVector.Zero;
            return;
        }

        if (!_initialPositionSet)
            SetInitialPosition();

        Motion.Update();

        if (_ignoreY)
            _body.Velocity = new JVector(Motion.CurrentMotion.X, -Motion.CurrentY, Motion.CurrentMotion.Z);
        else
            _body.Velocity = new JVector(Motion.CurrentMotion.X, Motion.CurrentMotion.Y, Motion.CurrentMotion.Z);

        _entity.LocalPosition = _body.Position.ToVector3();

        CollisionResponder.Update();
    }

    private void SetInitialPosition()
    {
        _body.Position = _entity.LocalPosition.ToJVector();
        _initialPositionSet = true;
    }

    public void RefreshPosition()
    {
        _body.Position = _entity.LocalPosition.ToJVector();
    }
}
