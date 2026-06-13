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
using System.Diagnostics;

namespace ExploringGame.Logics.Controllers;

public class TentacleController : IActiveObject
{
    private readonly Physics _physics;


    private RigidBody _body;

    public TentacleController(Physics physics)
    {
        _physics = physics;
    }

    public Tentacle Tentacle { get; set; }
    public Shape ConnectsTo { get; set; }

    public void Initialize()
    {
        _body = Tentacle.ColliderBodies[0];
        _body.SetMassInertia(0.001f);
        _physics.CreateHinge(_body, ConnectsTo.ColliderBodies[0], new Vector3(ConnectsTo.LocalPosition.X, ConnectsTo.GetWorldSide(Side.Top), ConnectsTo.LocalPosition.Z));
    }

    public void Stop()
    {
    }

    public void Update(GameTime gameTime)
    {
        if (GameDebug.Debug.NoNPCPhysics)
            return;

        Tentacle.LocalPosition = _body.Position.ToVector3();
        Tentacle.Rotation = new Rotation(_body.Orientation.ToQuaternion());

        _body.AngularVelocity = new JVector(10.0f, 5.0f, -4.0f);
    }
}