using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using Jitter2;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.Dynamics.Constraints;
using Jitter2.LinearMath;
using Microsoft.Xna.Framework;
using GShape = ExploringGame.GeometryBuilder.Shape;

namespace ExploringGame.Services;

public class Physics
{
    public const float WallColliderThickness = 0.5f;

    private World _world;
    public Physics()
    {
        _world = new World();
        
    }

    public RigidBody CreateStaticSurface(GShape shape, Side side)
    {
        var body = _world.CreateRigidBody();
        if (shape.Width == 0 || shape.Height == 0 || shape.Depth == 0)
            return null;

        // probably is a much cleaner way to do this
        switch(side)
        {
            case Side.Bottom:
                body.AddShape(new BoxShape(shape.Width, WallColliderThickness, shape.Depth));
                body.Position = new JVector(shape.X, shape.GetSide(Side.Bottom) - (WallColliderThickness / 2.0f), shape.Z); 
                break;
            case Side.Top:
                body.AddShape(new BoxShape(shape.Width, WallColliderThickness, shape.Depth));
                body.Position = new JVector(shape.X, shape.GetSide(Side.Top) + (WallColliderThickness / 2.0f), shape.Z);
                break;
            case Side.North:
                body.AddShape(new BoxShape(shape.Width, shape.Height, WallColliderThickness));
                body.Position = new JVector(shape.X, shape.Y, shape.GetSide(Side.North) - (WallColliderThickness / 2.0f));
                break;
            case Side.South:
                body.AddShape(new BoxShape(shape.Width, shape.Height, WallColliderThickness));
                body.Position = new JVector(shape.X, shape.Y, shape.GetSide(Side.South) + (WallColliderThickness / 2.0f));
                break;
            case Side.West:
                body.AddShape(new BoxShape(WallColliderThickness, shape.Height, shape.Depth));
                body.Position = new JVector(shape.GetSide(Side.West) - (WallColliderThickness / 2.0f), shape.Y, shape.Z);
                break;
            case Side.East:
                body.AddShape(new BoxShape(WallColliderThickness, shape.Height, shape.Depth));
                body.Position = new JVector(shape.GetSide(Side.East) + (WallColliderThickness / 2.0f), shape.Y, shape.Z);
                break;
        }

        body.MotionType = MotionType.Static;
        return body;
    }
    public RigidBody CreateStaticBody(GShape shape)
    {
        if (shape.Width == 0 || shape.Height == 0 || shape.Depth == 0)
            return null;

        var body = _world.CreateRigidBody();
        body.AddShape(new BoxShape(shape.Width, shape.Height, shape.Depth));

        if (shape.Rotation != null)
        {
            var rotationQ = shape.Rotation.AsQuaternion();
            body.Orientation = new JQuaternion(rotationQ.X, rotationQ.Y, rotationQ.Z, rotationQ.W);
        }
        body.Position = shape.Position.ToJVector();
        body.MotionType = MotionType.Static;
        return body;
    }

    private void InitPhysics(RigidBody body)
    {
        body.AffectedByGravity = false;
        body.Friction = 0;
        body.Damping = new(0f, 0f);
        body.SetMassInertia(1.0f);
    }

    public RigidBody CreateDynamicBody(GShape shape)
    {
        var body = _world.CreateRigidBody();
        body.AddShape(new BoxShape(shape.Width, shape.Height, shape.Depth));
        body.Position = shape.Position.ToJVector();

        InitPhysics(body);

        body.MotionType = MotionType.Dynamic;
        return body;
    }

    public RigidBody CreateCapsule(IWithPosition entity)
    {
        var body = _world.CreateRigidBody();
        body.AddShape(new CapsuleShape(0.4f, 2.0f)); //todo
        body.Position = entity.Position.ToJVector();
        body.MotionType = MotionType.Dynamic;

        InitPhysics(body);

        var upright = _world.CreateConstraint<HingeAngle>(body, _world.NullBody);
        upright.Initialize(JVector.UnitY, AngularLimit.Full);

        return body;
    }

    public RigidBody CreateHingedDoor(Door door)
    {
        var hinge = new Box();
        hinge.Height = 0.1f;
        hinge.Width = 0.1f;
        hinge.Depth = 0.1f;
        hinge.Position = door.Position;
        hinge.Place().OnSideOuter(Side.West, door);
        hinge.X -= 0.1f;

        hinge.MainTexture = new Texture.TextureInfo(Color.Blue);
        door.Parent.AddChild(hinge);

        
        var doorBody = CreateDynamicBody(door);
        var hingeBody = CreateStaticBody(hinge);
        InitPhysics(doorBody);
        doorBody.SetMassInertia(10.0f);


        var h = new HingeJoint(_world, hingeBody, doorBody, hinge.Position.ToJVector(), JVector.UnitY, AngularLimit.Full,
            hasMotor: false);
        
      //  h.Motor.IsEnabled = true;
     //   h.Motor.TargetVelocity = 20.0f;
      //  h.Motor.MaximumForce = 20.0f;

       // doorBody.Torque = new JVector(1.0f,0.0f, 0.0f); 
       // hingeBody.SetActivationState(false);

        //var hingeAngle = _world.CreateConstraint<HingeAngle>(hingeBody, doorBody);
        //hingeAngle.Initialize(JVector.UnitY, AngularLimit.FromDegree(0f,90f));
       
        //var ballSocket = _world.CreateConstraint<BallSocket>(hingeBody, doorBody);
        //ballSocket.Initialize(hinge.Position.ToJVector());

        return doorBody;
    }

    public void Update(GameTime gameTime)
    {
        if(gameTime.ElapsedGameTime.TotalSeconds > 0)
            _world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
    }

}
