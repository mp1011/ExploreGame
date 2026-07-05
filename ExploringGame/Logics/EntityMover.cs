using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using ExploringGame.Logics.Collision;
using ExploringGame.Motion;
using ExploringGame.Services;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.LinearMath;
using Microsoft.Xna.Framework;
using System.Linq;

namespace ExploringGame.Logics;

public class EntityMover : IActiveObject
{
    private readonly Physics _physics;
    private readonly bool _ignoreY;

    public Rotation TargetRotation { get; set; }
    public Vector3 AbsoluteAngularVelocity { get; set; }

    public AcceleratedMotion Motion { get; }
    private ICollidable _entity;
    private RigidBody _body;
    private bool _initialPositionSet = false;

    public CollisionResponder CollisionResponder { get; }

    public bool Active { get; set; }

    public EntityMover(ICollidable entity, Physics physics, bool ignoreY)
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
            _body.Velocity = new JVector(Motion.CurrentMotion.X, _body.Velocity.Y, Motion.CurrentMotion.Z);

        _entity.WorldPosition = _body.Position.ToVector3();

        if (TargetRotation != null)
            UpdateRotation();

        CollisionResponder.Update();
    }

    private void UpdateRotation()
    {        
        var currentRotation = new Rotation(_body.Orientation.ToQuaternion());

        var y1 = currentRotation.Yaw;
        var y2 = TargetRotation.Yaw;
        var dy = y1.ShortestRotation(AbsoluteAngularVelocity.Y, y2);


        var p1 = currentRotation.Pitch;
        var p2 = TargetRotation.Pitch;
        var dp = p1.ShortestRotation(AbsoluteAngularVelocity.X, p2);

        var r1 = currentRotation.Roll;
        var r2 = TargetRotation.Roll;
        var dr = r1.ShortestRotation(AbsoluteAngularVelocity.Z, r2);

        //X updates pitch
        //Y updates yaw
        //Z updates roll
        _body.AngularVelocity = new Jitter2.LinearMath.JVector(dp, dy, dr);
    }

    private void SetInitialPosition()
    {
        _body.Position = _entity.WorldPosition.ToJVector();
        _initialPositionSet = true;
    }

    public void RefreshPosition()
    {
        _body.Position = _entity.WorldPosition.ToJVector();
    }
}
