using ExploringGame.Logics;

namespace ExploringGame.GameDebug;

public interface IDebugControllable
{
    void DebugUpdate(IPlayerInput playerInput);
}
