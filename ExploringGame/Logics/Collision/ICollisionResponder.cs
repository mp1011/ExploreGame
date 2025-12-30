using Microsoft.Xna.Framework;

namespace ExploringGame.Logics.Collision;

public interface ICollisionResponder
{
    CollisionResponse CheckCollision(CollisionResponse lastResponse);
}
