using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GameDebug;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.Logics.Collision;
using ExploringGame.Story;
using Jitter2;
using Jitter2.Collision;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.Dynamics.Constraints;
using Jitter2.LinearMath;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using GShape = ExploringGame.GeometryBuilder.Shape;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace ExploringGame.Services;

[Flags]
public enum CollisionGroup
{
    None = 0,
    Player = 1,
    Environment = 2,
    Doors = 4,
    Steps = 8,
    SolidEntity = 16,
    LineOfSightTest = 32,
    Appendages = 64,
    MovingObjects = Player | SolidEntity,
    All = Player | Environment | Doors | Steps | SolidEntity
}

public record CollisionInfo(CollisionGroup MyGroup, CollisionGroup CollidesWithGroups, ICollidable Shape = null);

public record RaycastResult(IDynamicTreeProxy HitObject, Vector3 Normal, float Lambda);

public class Physics
{
    public const float WallColliderThickness = 0.5f;

    private World _world;
    public Physics()
    {
        _world = new World();
        _world.BroadPhaseFilter = new CollisionGroupFilter();
        _world.NarrowPhaseFilter = new CollisionModifier(_world.NarrowPhaseFilter);
    }

    public RaycastResult Raycast(ICollidable origin, ICollidable target)
    {
        var direction = Vector3.Normalize(target.WorldPosition - origin.WorldPosition).ToJVector();

        IDynamicTreeProxy proxy;
        JVector normal;
        float lambda;

        if (_world.DynamicTree.RayCast(origin.WorldPosition.ToJVector(), direction, 
            pre: p => {
                if (p.BelongsTo(origin))
                    return false;

                // prevent hitting non-colliding objects, unless that's what we're trying to do
                if (target.CollisionGroup != CollisionGroup.None && p.CollisionInfo().MyGroup == CollisionGroup.None)
                    return false;

                return true;
                },
            post: null, 
            proxy: out proxy, 
            normal: out normal, 
            lambda: out lambda))
        {
            return new RaycastResult(proxy, normal.ToVector3(), lambda);
        }
        else
            return new RaycastResult(null, Vector3.Zero, 0f);
    }

    public bool HasLineOfSight(ICollidable origin, ICollidable target)
    {
        var result = Raycast(origin, target);

        return result.HitObject != null && result.HitObject.BelongsTo(target);
    }

    public void Remove(RigidBody body)
    {
        _world.Remove(body);
    }
    
    public RigidBody CreateMeshShape(Vector3 worldOrigin, Triangle[] triangles)
    {
        triangles = triangles
            .Select(p => p.Invert())
            .InWorldCoordinates(worldOrigin);

        var body = _world.CreateRigidBody();

        var jTriangles = triangles.Where(p=>!p.IsDegenerate).Select(t => new JTriangle(
            new JVector(t.A.X, t.A.Y, t.A.Z),
            new JVector(t.B.X, t.B.Y, t.B.Z),
            new JVector(t.C.X, t.C.Y, t.C.Z)
            )).ToArray();

        var mesh = new TriangleMesh((IEnumerable<JTriangle>)jTriangles, ignoreDegenerated: false);

        body.AddShapes(Enumerable.Range(0, mesh.Indices.Length).Select(i => new TriangleShape(mesh, i)), MassInertiaUpdateMode.Preserve);

        body.MotionType = MotionType.Static;
        body.Tag = new CollisionInfo(CollisionGroup.Environment, CollisionGroup.Player | CollisionGroup.SolidEntity, null);
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
                body.Position = new JVector(shape.LocalX, shape.GetWorldSide(Side.Bottom) - (WallColliderThickness / 2.0f), shape.LocalZ); 
                break;
            case Side.Top:
                body.AddShape(new BoxShape(shape.Width, WallColliderThickness, shape.Depth));
                body.Position = new JVector(shape.LocalX, shape.GetWorldSide(Side.Top) + (WallColliderThickness / 2.0f), shape.LocalZ);
                break;
            case Side.North:
                body.AddShape(new BoxShape(shape.Width, shape.Height, WallColliderThickness));
                body.Position = new JVector(shape.LocalX, shape.LocalY, shape.GetWorldSide(Side.North) - (WallColliderThickness / 2.0f));
                break;
            case Side.South:
                body.AddShape(new BoxShape(shape.Width, shape.Height, WallColliderThickness));
                body.Position = new JVector(shape.LocalX, shape.LocalY, shape.GetWorldSide(Side.South) + (WallColliderThickness / 2.0f));
                break;
            case Side.West:
                body.AddShape(new BoxShape(WallColliderThickness, shape.Height, shape.Depth));
                body.Position = new JVector(shape.GetWorldSide(Side.West) - (WallColliderThickness / 2.0f), shape.LocalY, shape.LocalZ);
                break;
            case Side.East:
                body.AddShape(new BoxShape(WallColliderThickness, shape.Height, shape.Depth));
                body.Position = new JVector(shape.GetWorldSide(Side.East) + (WallColliderThickness / 2.0f), shape.LocalY, shape.LocalZ);
                break;
        }

        body.MotionType = MotionType.Static;
        body.Tag = new CollisionInfo(CollisionGroup.Environment, CollisionGroup.Player | CollisionGroup.SolidEntity);
        return body;
    }

    public RigidBody CreateStaticBody(ICollidable shape)
    {
        if (!shape.Size.IsValidPositive())
            return null;

        var body = CreateStaticBody(shape, shape.CollisionGroup, shape.CollidesWithGroups);
        body.Tag = (body.Tag as CollisionInfo) with { Shape = shape };
        return body;
    }

    /// <summary>
    /// Fixes the relative position of two rigid bodies based on their current position
    /// </summary>
    /// <param name="body"></param>
    /// <param name="weldTo"></param>
    public void Weld(GeometryBuilder.Shape body, GeometryBuilder.Shape weldTo, Vector3 weldPosition)
    {
        if (body.ColliderBodies[0].MotionType == MotionType.Static || weldTo.ColliderBodies[0].MotionType == MotionType.Static)
            throw new Exception("Physics joined shapes cannot be static");

        //   var constraint = _world.CreateConstraint<BallSocket>(body.ColliderBodies[0], weldTo.ColliderBodies[0]);
        //var constraint = _world.CreateConstraint<FixedAngle>(weldTo.ColliderBodies[0], body.ColliderBodies[0]);
      //  var constraint = _world.CreateConstraint<FixedAngle>(body.ColliderBodies[0], weldTo.ColliderBodies[0]);

     //   constraint.Initialize();
       // constraint.Initialize(weldPosition.ToJVector());

         var weldJoint = new WeldJoint(_world, body.ColliderBodies[0], weldTo.ColliderBodies[0], weldPosition.ToJVector());
//         var weldJoint = new WeldJoint(_world,  weldTo.ColliderBodies[0], body.ColliderBodies[0], weldPosition.ToJVector());
    }

    public void CreateHinge(GeometryBuilder.Shape body, GeometryBuilder.Shape other, Vector3 hingeWorldPosition)
    {
        if (body.ColliderBodies[0].MotionType == MotionType.Static || other.ColliderBodies[0].MotionType == MotionType.Static)
            throw new Exception("Physics joined shapes cannot be static");

        body.ColliderBodies[0].AffectedByGravity = true;

        var ballSocket = _world.CreateConstraint<BallSocket>(body.ColliderBodies[0], other.ColliderBodies[0]);
        ballSocket.Initialize(hingeWorldPosition.ToJVector());      
    }

    public RigidBody CreateStaticBody(IWithPosition shape, CollisionGroup myGroup, CollisionGroup collidesWithGroups)
    {
        if (!shape.Size.IsValidPositive())
            return null;

        if (shape.Size.X == 0 || shape.Size.Y == 0 || shape.Size.Z == 0)
            return null;

        var body = _world.CreateRigidBody();
        body.AddShape(new BoxShape(shape.Width(), shape.Height(), shape.Depth()));

        if (shape.Rotation != null)
        {
            body.Orientation = shape.Rotation.Quaternion.ToJQuaternion();
        }
        body.Position = shape.WorldPosition.ToJVector();
        body.MotionType = MotionType.Static;
        body.Tag = new CollisionInfo(myGroup, collidesWithGroups);
        return body;
    }

    private void InitPhysics(RigidBody body)
    {
        body.AffectedByGravity = false;
        body.Friction = 0;
        body.Damping = new(0f, 0f);
        body.SetMassInertia(1.0f);
    }

    public RigidBody CreateDynamicBody(ICollidable entity)
    {        
        var body = _world.CreateRigidBody();
        body.AddShape(new BoxShape(entity.Width(), entity.Height(), entity.Depth()));
        body.Position = entity.WorldPosition.ToJVector();

        InitPhysics(body);

        body.MotionType = MotionType.Dynamic;
        body.Tag = new CollisionInfo(entity.CollisionGroup, entity.CollidesWithGroups, entity);
        return body;
    }

    public RigidBody CreateCapsule(ICollidable entity, CollisionGroup myGroup, CollisionGroup collidesWithGroups, bool keepUpright)
    {
        var body = _world.CreateRigidBody();
        // body.AddShape(new CapsuleShape(0.4f, 1.0f)); //todo
        body.AddShape(new CapsuleShape(entity.Width() / 2f, entity.Height())); 


        body.Position = entity.WorldPosition.ToJVector();
        body.MotionType = MotionType.Dynamic;

        InitPhysics(body);

        if (keepUpright)
        {
            var upright = _world.CreateConstraint<HingeAngle>(body, _world.NullBody);
            upright.Initialize(JVector.UnitY, AngularLimit.Full);
        }

        body.Tag = new CollisionInfo(myGroup, collidesWithGroups, entity);
        return body;
    }

    public RigidBody CreateSphere(ICollidable entity, float radius, CollisionGroup myGroup, CollisionGroup collidesWithGroups)
    {
        var body = _world.CreateRigidBody();
        body.AddShape(new SphereShape(radius));
        body.Position = entity.WorldPosition.ToJVector();
        body.MotionType = MotionType.Dynamic;

        InitPhysics(body);

        body.Tag = new CollisionInfo(myGroup, collidesWithGroups, entity);
        return body;
    }

    public RigidBody CreateSphere(ICollidable entity, float radius)
    {
        return CreateSphere(entity, radius, entity.CollisionGroup, entity.CollidesWithGroups);
    }


    public RigidBody CreateSlidingDoor(SlidingDoorPane pane, Side openSide)
    {
        var doorBody = CreateDynamicBody(pane);
        doorBody.SetMassInertia(1.0f);

        var openAnchor = new Box();
        openAnchor.Height = 0.01f;
        openAnchor.Width = 0.01f;
        openAnchor.Depth = 0.01f;
        openAnchor.WorldPosition = pane.WorldPosition + openSide.AsVector() * 2.0f;

        var anchorBody = CreateStaticBody(openAnchor, CollisionGroup.Environment, CollisionGroup.None);
        
        new PrismaticJoint(_world, doorBody, anchorBody, doorBody.Position, openSide.AsVector().ToJVector(), pinned: true, hasMotor: false);

        //var constraint = _world.CreateConstraint<PointOnLine>(doorBody, _world.NullBody);
        //constraint.Initialize(
        //    new JVector(0f, 1f, 0f), 
        //    pane.Position.ToJVector(), 
        //    new JVector(pane.Position.X - 1.0f, pane.Position.Y, pane.Position.Z), 
        //    LinearLimit.Fixed);

        return doorBody;
    }

    public RigidBody CreateHingedDoor(Door door)
    {
        var hinge = new Box();
        hinge.Height = 1.00f;
        hinge.Width = 0.01f;
        hinge.Depth = 0.01f;
        hinge.WorldPosition = door.WorldPosition;

        if (door.HingePosition == HAlign.Left)
        {
            hinge.Place().OnSideOuter(Side.West, door);
            hinge.LocalX -= 0.01f;
        }
        else
        {
            hinge.Place().OnSideOuter(Side.East, door);
            hinge.LocalX += 0.01f;
        }

        var doorBody = CreateDynamicBody(door);
        var hingeBody = CreateStaticBody(hinge, CollisionGroup.Environment, CollisionGroup.None);
        InitPhysics(doorBody);
        doorBody.SetMassInertia(10000.0f);

        var minAngle = MathHelper.Min(door.OpenAngle.Degrees, door.ClosedAngle.Degrees);
        var maxAngle = MathHelper.Max(door.OpenAngle.Degrees, door.ClosedAngle.Degrees);

        // note, seems to work better if we limit the angle ourselves
        var h = new HingeJoint(_world, hingeBody, doorBody, hinge.WorldPosition.ToJVector(), JVector.UnitY, 
            AngularLimit.Full,
           // AngularLimit.FromDegree(minAngle, maxAngle),
            hasMotor: false);

        doorBody.Orientation = door.Rotation.Quaternion.ToJQuaternion();
        doorBody.Tag = new CollisionInfo(door.CollisionGroup, door.CollidesWithGroups, door);

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
        if (Debug.NoPhysics)
            return;

        if(gameTime.ElapsedGameTime.TotalSeconds > 0)
            _world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
    }

    public string RigidBodyDiagnostics()
    {
        return String.Join(Environment.NewLine,
            _world.RigidBodies.Select(p => p.DiagnosticInfo()).ToArray());
    }

    class CollisionGroupFilter : IBroadPhaseFilter
    {
        public bool Filter(IDynamicTreeProxy proxyA, IDynamicTreeProxy proxyB)
        {
            if(proxyA is RigidBodyShape bodyA && proxyB is RigidBodyShape bodyB)
            {
                var infoA = bodyA.RigidBody.Tag as CollisionInfo;
                var infoB = bodyB.RigidBody.Tag as CollisionInfo;

                if (infoA == null || infoB == null)
                    return false;

                return IsCollisionAllowed(infoA, infoB);
            }

            return false;
        }

        private bool IsCollisionAllowed(CollisionInfo infoA, CollisionInfo infoB)
        {
            if(infoA.MyGroup == CollisionGroup.None || infoB.MyGroup == CollisionGroup.None)
                return false;

            // Check if A can collide with B's group and B can collide with A's group
            bool aCollidesWithB = (infoA.CollidesWithGroups & infoB.MyGroup) != 0;
            bool bCollidesWithA = (infoB.CollidesWithGroups & infoA.MyGroup) != 0;
            
            // Both must allow the collision
            if (!aCollidesWithB || !bCollidesWithA)
                return false;
            
            // Special case: respect FlyMode for Player + Environment collisions
            if ((infoA.MyGroup == CollisionGroup.Player && infoB.MyGroup == CollisionGroup.Environment) ||
                (infoA.MyGroup == CollisionGroup.Environment && infoB.MyGroup == CollisionGroup.Player))
            {
                return !Debug.FlyMode;
            }
            
            return true;
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
                var infoA = shapeA.RigidBody.Tag as CollisionInfo;
                var infoB = shapeB.RigidBody.Tag as CollisionInfo;
                
                if (infoA?.MyGroup == CollisionGroup.Steps)
                    HandleStep(playerShape: shapeB, stepShape: shapeA);
                else if (infoB?.MyGroup == CollisionGroup.Steps)
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
