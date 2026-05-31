using ExploringGame.Entities;
using ExploringGame.Entities.EntityParts;
using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using ExploringGame.Logics.Collision;
using ExploringGame.Services;
using ExploringGame.Story.Scene01.Act01;
using Jitter2.Dynamics;
using Jitter2.LinearMath;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.Logics.Controllers;

public class PuppetController : IActiveObject
{
    private readonly Physics _physics;
    private readonly Random _rng;

    private EntityMover _mover;
    
    public PuppetController(Physics physics, Random random)
    {
        _physics = physics;
        _rng = random;
    }

    public Puppet Puppet { get; set; }

    public void Initialize()                                      
    {
        _mover = new EntityMover(Puppet, _physics, ignoreY: false);
        _mover.Initialize();

        _mover.Motion.Acceleration = 0.1f;
        _mover.Motion.TargetMotion = new Vector3(1.0f, 0.0f, 1.0f);

        // don't like this
        Puppet.ColliderBodies[0].SetMassInertia(10f);
    }

    public void Stop()
    {
    }

    private double t = 0;
    public void Update(GameTime gameTime)
    {
        _mover.Update(gameTime);

        t += gameTime.ElapsedGameTime.TotalSeconds;

        if (t >= 2.0)
        {
            t = 0;
            _mover.Motion.TargetMotion = new Vector3(10 * ((float)_rng.NextDouble() - 0.5f), 2 * ((float)_rng.NextDouble() - 0.5f), 10 * ((float)_rng.NextDouble() - 0.5f));
        }
    }
}

// unsure about this 
public class PuppetPartController : IActiveObject
{
    private readonly Physics _physics;


    private RigidBody _body;

    public PuppetPartController(Physics physics)
    {
        _physics = physics;
    }

    public TestArm PuppetPart { get; set; }

    public void Initialize()
    {
        _body = PuppetPart.ColliderBodies[0];
        _physics.CreateHinge(_body, PuppetPart.Entity.ColliderBodies[0], PuppetPart.Entity.Position);
    }

    public void Stop()
    {
    }

    public void Update(GameTime gameTime)
    {
        _body.AngularVelocity = new JVector(0.1f, 0f, 0f);
        PuppetPart.Position = _body.Position.ToVector3();
        PuppetPart.Rotation = new Rotation(_body.Orientation.ToQuaternion());
    }
}