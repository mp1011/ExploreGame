using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace ExploringGame.Logics;

public enum GameKey
{
    Crouch,
    Run,
    Jump,
    Use,
    StrafeLeft,
    StrafeRight,
    Forward,
    Backward,
    DebugKey,
}

public class PlayerInput : IPlayerInput
{
    private KeyboardState _lastKeyboardState;
    private KeyboardState _currentKeyboardState;
    private MouseState _lastMouseState;
    private MouseState _currentMouseState;
    private Dictionary<GameKey, Keys> _keyMap;
    private bool _firstMouse = true;

    public PlayerInput()
    {
        _keyMap = new Dictionary<GameKey, Keys>();
        _keyMap[GameKey.Run] = Keys.LeftShift;
        _keyMap[GameKey.Crouch] = Keys.LeftControl;
        _keyMap[GameKey.Jump] = Keys.Space;
        _keyMap[GameKey.Use] = Keys.E;

        _keyMap[GameKey.Forward] = Keys.W;
        _keyMap[GameKey.Backward] = Keys.S;
        _keyMap[GameKey.StrafeLeft] = Keys.A;
        _keyMap[GameKey.StrafeRight] = Keys.D;
        
        _keyMap[GameKey.DebugKey] = Keys.RightAlt;
    }

    public void Update(GameWindow window)
    {
        _lastKeyboardState = _currentKeyboardState;
        _currentKeyboardState = Keyboard.GetState();

        _lastMouseState = _currentMouseState;
        _currentMouseState = Mouse.GetState();

        if (_firstMouse)
        {
            CenterMouse(window);
            _lastMouseState = _currentMouseState;
            _firstMouse = false;
        }
    }

    public bool IsKeyPressed(Keys key) => _currentKeyboardState.IsKeyDown(key) && !_lastKeyboardState.IsKeyDown(key);
    public bool IsKeyDown(GameKey key) => _currentKeyboardState.IsKeyDown(_keyMap[key]);
    public bool IsKeyPressed(GameKey key) => _currentKeyboardState.IsKeyDown(_keyMap[key])
                                             && !_lastKeyboardState.IsKeyDown(_keyMap[key]);

    public Vector2 GetMouseDelta()
    {
        if (_firstMouse)
            return Vector2.Zero;

        var delta = _currentMouseState.Position - _lastMouseState.Position;
        return new Vector2(delta.X, delta.Y);
    }

    public void CenterMouse(GameWindow window)
    {
        Mouse.SetPosition(window.ClientBounds.Width / 2, window.ClientBounds.Height / 2);
        _currentMouseState = Mouse.GetState();
    }
}
