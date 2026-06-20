using ExploringGame.Entities.EntityParts;
using ExploringGame.GeometryBuilder;
using ExploringGame.Services;
using Jitter2.Dynamics;
using Microsoft.Xna.Framework;

namespace ExploringGame.Logics.Controllers;

public class PuppetArmController : IActiveObject
{
    private readonly Physics _physics;


    private RigidBody _body;

    public PuppetArmController(Physics physics)
    {
        _physics = physics;
    }

    public BasicArmPart ArmPart { get; set; }
    public Shape ConnectsTo { get; set; }

    public void Initialize()
    {
        _body = ArmPart.ColliderBodies[0];
        _body.AffectedByGravity = true;
        _body.SetMassInertia(0.001f);

        ArmPart.InitializePhysicsObject();
        _physics.CreateHinge(ConnectsTo, ArmPart, new Vector3(ConnectsTo.WorldPosition.X, ConnectsTo.GetWorldSide(Side.Top), ConnectsTo.WorldPosition.Z));
    }

    public void Stop()
    {
    }

    public void Update(GameTime gameTime)
    {
        if (GameDebug.Debug.NoNPCPhysics)
            return;

        ArmPart.SyncShapePosition();
    }
}