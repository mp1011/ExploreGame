using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using Jitter2;
using Jitter2.Collision;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.Dynamics.Constraints;
using Jitter2.LinearMath;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using GShape = ExploringGame.GeometryBuilder.Shape;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace ExploringGame.Services;

public enum CollisionGroup
{
    Player = 1,
    Environment = 2,
    Doors = 4,
    Steps = 8,
}

public class Physics
{
    public static bool DebugDrawHinge = false;

    public const float WallColliderThickness = 0.5f;

    private World _world;
    public Physics()
    {
        _world = new World();
        _world.BroadPhaseFilter = new CollisionGroupFilter();
        _world.NarrowPhaseFilter = new CollisionModifier(_world.NarrowPhaseFilter);
    }

    public void Remove(RigidBody body)
    {
        _world.Remove(body);
    }
    
    public RigidBody CreateMeshShape(Triangle[] triangles)
    {
        triangles = triangles.Select(p=>p.Invert()).ToArray();
        var body = _world.CreateRigidBody();

        var jTriangles = triangles.Where(p=>!p.IsDegenerate).Select(t => new JTriangle(
            new JVector(t.A.X, t.A.Y, t.A.Z),
            new JVector(t.B.X, t.B.Y, t.B.Z),
            new JVector(t.C.X, t.C.Y, t.C.Z)
            )).ToArray();

        var mesh = new TriangleMesh(jTriangles);
        body.AddShape(Enumerable.Range(0, mesh.Indices.Length).Select(i => new TriangleShape(mesh, i)), setMassInertia: false);
        body.MotionType = MotionType.Static;
        body.Tag = CollisionGroup.Environment;
        return body;
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
        body.Tag = CollisionGroup.Environment;
        return body;
    }
    public RigidBody CreateStaticBody(GShape shape, CollisionGroup collisionGroup = CollisionGroup.Environment)
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
        body.Tag = collisionGroup;
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
        body.Tag = CollisionGroup.Environment;
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

        body.Tag = CollisionGroup.Player;
        return body;
    }

    public RigidBody CreateHingedDoor(Door door)
    {
        var hinge = new Box();
        hinge.Height = 1.00f;
        hinge.Width = 0.01f;
        hinge.Depth = 0.01f;
        hinge.Position = door.Position;

        if (door.HingePosition == HAlign.Left)
        {
            hinge.Place().OnSideOuter(Side.West, door);
            hinge.X -= 0.01f;
        }
        else
        {
            hinge.Place().OnSideOuter(Side.East, door);
            hinge.X += 0.01f;
        }

        if (DebugDrawHinge)
        {
            throw new Exception("this needs to be redone");
            hinge.MainTexture = new Texture.TextureInfo(Color.Pink);
            door.Parent.AddChild(hinge);
        }
        
        var doorBody = CreateDynamicBody(door);
        var hingeBody = CreateStaticBody(hinge);
        InitPhysics(doorBody);
        doorBody.SetMassInertia(10.0f);

        var minAngle = MathHelper.Min(door.OpenAngle.Degrees, door.ClosedAngle.Degrees);
        var maxAngle = MathHelper.Max(door.OpenAngle.Degrees, door.ClosedAngle.Degrees);

        // note, seems to work better if we limit the angle ourselves
        var h = new HingeJoint(_world, hingeBody, doorBody, hinge.Position.ToJVector(), JVector.UnitY, 
            AngularLimit.Full,
           // AngularLimit.FromDegree(minAngle, maxAngle),
            hasMotor: false);

        var rotationQ = door.Rotation.AsQuaternion();
        doorBody.Orientation = new JQuaternion(rotationQ.X, rotationQ.Y, rotationQ.Z, rotationQ.W);
        doorBody.Tag = CollisionGroup.Doors;

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


    class CollisionGroupFilter : IBroadPhaseFilter
    {
        public bool Filter(IDynamicTreeProxy proxyA, IDynamicTreeProxy proxyB)
        {
            if(proxyA is RigidBodyShape bodyA && proxyB is RigidBodyShape bodyB)
            {
               return IsCollisionAllowed((CollisionGroup)bodyA.RigidBody.Tag,
                                         (CollisionGroup)bodyB.RigidBody.Tag);
            }

            return false;
        }

        private bool IsCollisionAllowed(CollisionGroup groupA, CollisionGroup groupB)
        {
            if (groupA == CollisionGroup.Player && groupB == CollisionGroup.Environment)
                return true;
            if (groupA == CollisionGroup.Environment && groupB == CollisionGroup.Player)
                return true;
            if (groupA == CollisionGroup.Doors && groupB == CollisionGroup.Player)
                return true;
            if (groupA == CollisionGroup.Player && groupB == CollisionGroup.Doors)
                return true;
            if (groupA == CollisionGroup.Player && groupB == CollisionGroup.Steps)
                return true;
            if (groupA == CollisionGroup.Steps && groupB == CollisionGroup.Player)
                return true;
            return false;
        }
    }

    class CollisionModifier : INarrowPhaseFilter
    {
        private INarrowPhaseFilter _default;

        public CollisionModifier(INarrowPhaseFilter defaultFilter)
        {
            _default = defaultFilter;
        }

        public bool Filter(RigidBodyShape shapeA, RigidBodyShape shapeB, ref JVector pointA, ref JVector pointB, ref JVector normal, ref float penetration)
        {
            var baseResult = _default.Filter(shapeA, shapeB, ref pointA, ref pointB, ref normal, ref penetration);

            if (normal.Y < 0.6 && normal.Y > -0.6)
            {
                if ((CollisionGroup)shapeA.RigidBody.Tag == CollisionGroup.Steps)
                    HandleStep(playerShape: shapeB, stepShape: shapeA);
                else if ((CollisionGroup)shapeB.RigidBody.Tag == CollisionGroup.Steps)
                    HandleStep(playerShape: shapeA, stepShape: shapeB);
            }
            return baseResult;
        }

        private void HandleStep(RigidBodyShape playerShape, RigidBodyShape stepShape)
        {
            var playerBody = playerShape.RigidBody;
            playerBody.Velocity += new JVector(0, 3.0f, 0); // Adjust Y value as needed for your step height
        }
    }
}
