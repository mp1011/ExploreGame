using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using Jitter2;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.LinearMath;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Runtime.InteropServices;
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
        body.Position = shape.Position.ToJVector();
        body.MotionType = MotionType.Static;
        return body;
    }

    public RigidBody CreateDynamicBody(GShape shape)
    {
        var body = _world.CreateRigidBody();
        body.AddShape(new BoxShape(shape.Width, shape.Height, shape.Depth));
        body.Position = shape.Position.ToJVector();
        body.MotionType = MotionType.Dynamic;
        return body;
    }

    public RigidBody CreateSphere(IWithPosition entity)
    {
        var body = _world.CreateRigidBody();
        body.AddShape(new SphereShape(1.0f));
        body.Position = entity.Position.ToJVector();
        body.MotionType = MotionType.Dynamic;
        return body;
    }

    public void Update(GameTime gameTime)
    {
        if(gameTime.ElapsedGameTime.TotalSeconds > 0)
            _world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
    }

}
