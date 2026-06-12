using ExploringGame.Camera;
using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;

namespace ExploringGame.GameDebug;

public class DebugBirdsEyeCamera : ICamera
{
    public Vector3 WorldPosition { get; set; }
    public Rotation Rotation { get; set; }

    public Vector3 Size => new Vector3(1, 1, 1);

    private Player _player;

    public DebugBirdsEyeCamera(Player player)
    {
        _player = player;
    }

    public Matrix CreateViewMatrix()
    {
        WorldPosition = new Vector3(_player.WorldPosition.X, 20f, _player.WorldPosition.Z);
        Quaternion orientation = Quaternion.CreateFromYawPitchRoll(0f, -MathHelper.PiOver2, 0f);
        Vector3 forward = Vector3.Transform(Vector3.Forward, orientation);
        Vector3 up = Vector3.Transform(Vector3.Up, orientation);

        return Matrix.CreateLookAt(WorldPosition, WorldPosition + forward, up);
    }
}
