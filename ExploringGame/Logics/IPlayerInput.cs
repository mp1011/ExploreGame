using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ExploringGame.Logics;

public interface IPlayerInput
{
    void Update(GameWindow window);
    bool IsKeyPressed(Keys key);
    bool IsKeyDown(GameKey key);
    bool IsKeyPressed(GameKey key);
    Vector2 GetMouseDelta();
    void CenterMouse(GameWindow window);
}
