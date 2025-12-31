using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace ExploringGame.Logics;

public enum GameKey
{
    Crouch,
    Run,
    Jump,
    StrafeLeft,
    StrafeRight,
    Forward,
    Backward,
    DebugToggleCollision
}

public class PlayerInput
{
    private KeyboardState _lastState;
    private KeyboardState _currentState;
    private Dictionary<GameKey, Keys> _keyMap;

    public PlayerInput()
    {
        _keyMap = new Dictionary<GameKey, Keys>();
        _keyMap[GameKey.Run] = Keys.LeftShift;
        _keyMap[GameKey.Crouch] = Keys.LeftControl;
        _keyMap[GameKey.Jump] = Keys.Space;

        _keyMap[GameKey.Forward] = Keys.W;
        _keyMap[GameKey.Backward] = Keys.S;
        _keyMap[GameKey.StrafeLeft] = Keys.A;
        _keyMap[GameKey.StrafeRight] = Keys.D;
        _keyMap[GameKey.DebugToggleCollision] = Keys.C;

    }

    public void Update()
    {
        _lastState = _currentState;
        _currentState = Keyboard.GetState();

    }

    public bool IsKeyDown(GameKey key) => _currentState.IsKeyDown(_keyMap[key]);
    public bool IsKeyPressed(GameKey key) => _currentState.IsKeyDown(_keyMap[key])
                                             && !_lastState.IsKeyDown(_keyMap[key]);

}
