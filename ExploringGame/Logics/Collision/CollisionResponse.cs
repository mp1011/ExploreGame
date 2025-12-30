using Microsoft.Xna.Framework;

namespace ExploringGame.Logics.Collision;

public record CollisionResponse(Vector3 OriginalPosition, Vector3 NewPosition, bool IgnoreWallCollision)
{
    public static CollisionResponse None(Vector3 OriginalPosition) => new CollisionResponse(OriginalPosition, OriginalPosition, false);
}
