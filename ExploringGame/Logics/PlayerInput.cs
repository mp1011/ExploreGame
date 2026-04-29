using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace ExploringGame.Logics;

public enum GameKey
{
    None,
    Crouch,
    Run,
    Jump,
    Use,
    StrafeLeft,
    StrafeRight,
    Forward,
    Backward,
    DebugKey,
    DialogueAdvance
}

public enum MouseButton
{
    Left,
    Middle,
    Right
}

public class PlayerInput : IPlayerInput
{
    private KeyboardState _lastKeyboardState;
    private KeyboardState _currentKeyboardState;
    private MouseState _lastMouseState;
    private MouseState _currentMouseState;
    private Dictionary<GameKey, Keys> _keyMap;
    private Dictionary<GameKey, MouseButton> _mouseMap;
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

        _mouseMap = new Dictionary<GameKey, MouseButton>();
        _mouseMap[GameKey.DialogueAdvance] = MouseButton.Left;
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

    private bool WasKeyDown(GameKey key) => IsKeyDown(key, _lastKeyboardState, _lastMouseState);
    private bool IsKeyDown(GameKey key, KeyboardState keyboardState, MouseState mouseState)
    {
        if(_keyMap.ContainsKey(key) && keyboardState.IsKeyDown(_keyMap[key]))
            return true;

        if(!_mouseMap.ContainsKey(key)) 
            return false;

        var mouseButton = _mouseMap[key];
        switch(mouseButton)
        {
            case MouseButton.Left:
                return mouseState.LeftButton == ButtonState.Pressed;
            case MouseButton.Right:
                return mouseState.RightButton == ButtonState.Pressed;
            case MouseButton.Middle:
                return mouseState.MiddleButton == ButtonState.Pressed;
            default:
                return false;
        }
    }

    public bool IsKeyPressed(Keys key) => _currentKeyboardState.IsKeyDown(key) && !_lastKeyboardState.IsKeyDown(key);
    public bool IsKeyDown(GameKey key) => IsKeyDown(key, _currentKeyboardState, _currentMouseState);
    public bool IsKeyPressed(GameKey key) => IsKeyDown(key) && !WasKeyDown(key);

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
