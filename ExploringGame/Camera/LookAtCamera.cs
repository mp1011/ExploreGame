using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.Camera;

public class LookAtCamera : ICamera
{
    private const float _deltaPerFrame = 0.05f;

    private readonly Shape _lookAt;
    private Matrix _lastView;

    public LookAtCamera(ICamera previous, Shape lookAt)
    {
        _lookAt = lookAt;
        _lastView = previous.CreateViewMatrix();
        Position = previous.Position;
        Rotation = previous.Rotation;
    }

    public Vector3 Position { get; set; }

    public Vector3 Size => Vector3.One;

    public Rotation Rotation { get; set; }

    public Matrix CreateViewMatrix()
    {
        var currentWorld = Matrix.Invert(_lastView);
        var cameraPosition = currentWorld.Translation;
        Position = cameraPosition;

        var targetDirection = _lookAt.Position - cameraPosition;
        if (targetDirection.LengthSquared() <= float.Epsilon)
            return _lastView;

        var targetView = Matrix.CreateLookAt(cameraPosition, _lookAt.Position, Vector3.Up);
        var currentRotation = Quaternion.CreateFromRotationMatrix(currentWorld);
        var targetRotation = Quaternion.CreateFromRotationMatrix(Matrix.Invert(targetView));
        var angularDistance = QuaternionAngle(currentRotation, targetRotation);

        if (angularDistance <= _deltaPerFrame)
        {
            _lastView = targetView;
            Rotation = RotationFromView(_lastView);
            return _lastView;
        }

        var step = _deltaPerFrame / angularDistance;
        var nextRotation = Quaternion.Normalize(Quaternion.Slerp(currentRotation, targetRotation, step));
        var rotationMatrix = Matrix.CreateFromQuaternion(nextRotation);
        var forward = Vector3.Normalize(Vector3.Transform(Vector3.Forward, rotationMatrix));
        var up = Vector3.Normalize(Vector3.Transform(Vector3.Up, rotationMatrix));

        _lastView = Matrix.CreateLookAt(cameraPosition, cameraPosition + forward, up);
        Rotation = RotationFromView(_lastView);
        return _lastView;
    }

    private static float QuaternionAngle(Quaternion current, Quaternion target)
    {
        var dot = Math.Clamp(MathF.Abs(Quaternion.Dot(current, target)), 0f, 1f);
        return 2f * MathF.Acos(dot);
    }

    private static Rotation RotationFromView(Matrix view)
    {
        var world = Matrix.Invert(view);
        var forward = Vector3.Normalize(Vector3.Transform(Vector3.Forward, world));

        return new Rotation(
            Yaw: MathF.Atan2(forward.X, forward.Z),
            Pitch: MathF.Asin(Math.Clamp(forward.Y, -1f, 1f)),
            Roll: 0f);
    }
}
