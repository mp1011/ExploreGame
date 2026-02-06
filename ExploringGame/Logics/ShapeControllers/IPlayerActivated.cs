using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;

namespace ExploringGame.Logics.ShapeControllers;

public interface IPlayerActivated
{
    float ActivationRange { get; }
    IPlayerInput PlayerInput { get; }
    Player Player { get; }
    Shape Shape { get; }
}

public static class IPlayerActivatedExtensions
{
    public static bool CheckPlayerActivation(this IPlayerActivated playerActivated)
    {      
        if (playerActivated.Player.Position.SquaredDistance(playerActivated.Shape.Position) > playerActivated.ActivationRange * playerActivated.ActivationRange)
            return false;

        // Angular check: ensure player is looking roughly at the shape
        var forward = Vector3.Transform(Vector3.Forward, Matrix.CreateFromYawPitchRoll(playerActivated.Player.Rotation.Yaw, playerActivated.Player.Rotation.Pitch, 0f));
        var toTarget = playerActivated.Shape.Position - playerActivated.Player.Position;

        forward.Normalize();
        toTarget.Normalize();

        const float lookThreshold = 0.50f;
        var dot = Vector3.Dot(forward, toTarget);

        if (!playerActivated.PlayerInput.IsKeyPressed(GameKey.Use))
            return false;

        return dot >= lookThreshold;
    }
}