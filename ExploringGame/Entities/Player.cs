using ExploringGame.GeometryBuilder;
using ExploringGame.Logics;
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

    private float PhysicsCapsuleHeight => 2.8f; // Capsule length (2.0) + 2 * radius (0.4) from Physics.CreateCapsule
    public static readonly float EyeHeight = Measure.Feet(5.5f);

    public CollisionGroup CollisionGroup => CollisionGroup.Player;

    public CollisionGroup CollidesWithGroups => CollisionGroup.All & ~CollisionGroup.Player;

    public RigidBody[] ColliderBodies { get; }

    public EntityMover Mover { get; set; }

    public Matrix CreateViewMatrix()
    {
        // Camera should be at eye height above the floor
        // Position is at center of physics capsule, so feet are at Position.Y - (PhysicsCapsuleHeight / 2f)
        var feetPosition = Position.Y - (PhysicsCapsuleHeight / 2f);
        var cameraY = feetPosition + EyeHeight;
        var cameraPosition = new Vector3(Position.X, cameraY, Position.Z);

        var lookDir = Vector3.Transform(Vector3.Forward, Matrix.CreateFromYawPitchRoll(Rotation.Yaw, Rotation.Pitch, 0));
        return Matrix.CreateLookAt(cameraPosition, cameraPosition + lookDir, Vector3.Up);
    }

    public Player(Physics physics)
    {
        ColliderBodies = new CapsuleColliderMaker(this).CreateColliders(physics).ToArray();
    }

    public override string ToString() => "Player";
}
