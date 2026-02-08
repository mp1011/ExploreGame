using ExploringGame.GeometryBuilder;
using ExploringGame.Logics.Collision;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using Jitter2.Dynamics;
using Microsoft.Xna.Framework;
using System.Linq;

namespace ExploringGame.Entities;

public class Player : ICollidable, ICamera
{
    public Vector3 Position { get; set; } = new Vector3(0, 1.5f, 0);
    public Rotation Rotation { get; set; } = new Rotation(0, 0.1f, 0);
    public int Health { get; set; } = 100;

    public Vector3 Size => new Vector3(0.5f, 1.8f, 0.5f);

    public CollisionGroup CollisionGroup => CollisionGroup.Player;

    public CollisionGroup CollidesWithGroups => CollisionGroup.All & ~CollisionGroup.Player;

    public RigidBody[] ColliderBodies { get; }

    public Matrix CreateViewMatrix()
    {
        var lookDir = Vector3.Transform(Vector3.Forward, Matrix.CreateFromYawPitchRoll(Rotation.Yaw, Rotation.Pitch, 0));
        return Matrix.CreateLookAt(Position, Position + lookDir, Vector3.Up);
    }

    public Player(Physics physics)
    {
        ColliderBodies = new CapsuleColliderMaker(this).CreateColliders(physics).ToArray();
    }

    public override string ToString() => "Player";
}
