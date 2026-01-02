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
    private World _world;
    public Physics()
    {
        _world = new World();
        
    }

    public RigidBody CreateStaticSurface(GShape shape, Side side)
    {
        var body = _world.CreateRigidBody();

        float thickness = 0.5f;

        switch(side)
        {
            case Side.North:
            case Side.South:
                body.AddShape(new BoxShape(shape.Width, shape.Height, thickness));
                break;
            case Side.West:
            case Side.East:
                body.AddShape(new BoxShape(thickness, shape.Height, shape.Depth));
                break;
        }

        body.Position = side switch
        {
            Side.North => new JVector(shape.X, shape.Y, shape.GetSide(Side.North) - (thickness / 2.0f)),
            Side.South => new JVector(shape.X, shape.Y, shape.GetSide(Side.South) + (thickness / 2.0f)),
            Side.West => new JVector(shape.GetSide(Side.West) - (thickness / 2.0f), shape.Y, shape.Z),
            Side.East => new JVector(shape.GetSide(Side.East) + (thickness / 2.0f), shape.Y, shape.Z),
            _ => throw new ArgumentException("invalid side")
        };

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

    public void Update(GameTime gameTime)
    {
        if(gameTime.ElapsedGameTime.TotalSeconds > 0)
            _world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
    }

}
