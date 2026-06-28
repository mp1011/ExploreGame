using ExploringGame.Camera;
using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;

namespace ExploringGame.GameDebug;

/// <summary>
/// A fixed camera for visual tests and debugging - no physics, just a static viewpoint.
/// </summary>
public class DebugFixedCamera : ICamera
{
    public Vector3 WorldPosition { get; set; }
    public Rotation Rotation { get; set; }

    public Vector3 Size { get; set; } = new Vector3(1, 1, 1);

    public DebugFixedCamera(Vector3 position, Rotation rotation)
    {
        WorldPosition = position;
        Rotation = rotation;
    }

    public DebugFixedCamera() : this(Vector3.Zero, new Rotation(0, 0, 0))
    {
    }

    public Matrix CreateViewMatrix()
    {
        var lookDir = Vector3.Transform(Vector3.Forward, Rotation.AsMatrix());
        return Matrix.CreateLookAt(WorldPosition, WorldPosition + lookDir, Vector3.Up);
    }

    /// <summary>
    /// Points the camera to look at a specific position.
    /// </summary>
    public void LookAt(Vector3 target)
    {
        var direction = Vector3.Normalize(target - WorldPosition);
        Rotation = new Rotation(
            Yaw: (float)System.Math.Atan2(-direction.X, -direction.Z),
            Pitch: (float)System.Math.Asin(direction.Y),
            Roll: 0f
        );
    }
}
