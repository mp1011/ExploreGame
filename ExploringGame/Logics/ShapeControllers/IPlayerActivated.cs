using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.Logics.Collision;
using ExploringGame.Services;
using Microsoft.Xna.Framework;

namespace ExploringGame.Logics.ShapeControllers;

public interface IPlayerActivated
{
    float ActivationRange { get; }
    IPlayerInput PlayerInput { get; }
    Player Player { get; }
    ICollidable Shape { get; }
}

public static class IPlayerActivatedExtensions
{
    public static bool CheckPlayerActivation(this IPlayerActivated playerActivated, Physics physics, GameKey key = GameKey.Use)
    {
        if (playerActivated.Player.WorldPosition.SquaredDistance(playerActivated.Shape.WorldPosition) > playerActivated.ActivationRange * playerActivated.ActivationRange)
            return false;

        if (!playerActivated.PlayerInput.IsKeyPressed(key))
            return false;

        if (!physics.HasLineOfSight(playerActivated.Player, playerActivated.Shape))
            return false;

        // Angular check: ensure player is looking roughly at the shape
        var forward = Vector3.Transform(Vector3.Forward, Matrix.CreateFromYawPitchRoll(playerActivated.Player.Rotation.Yaw.Radians, 0f, 0f));
        var toTarget = playerActivated.Shape.WorldPosition - playerActivated.Player.WorldPosition;
        forward.Normalize();
        toTarget.Normalize();

        const float lookThreshold = 0.50f;
        var dot = Vector3.Dot(forward, toTarget);

        return dot >= lookThreshold;
    }
}