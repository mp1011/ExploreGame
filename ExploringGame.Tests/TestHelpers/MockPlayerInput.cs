using ExploringGame.Logics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ExploringGame.Tests.TestHelpers;

public class MockPlayerInput : IPlayerInput
{
    private int _currentFrame = 0;
    private List<InputEvent> _events = new();
    private readonly HashSet<GameKey> _pressedKeys = new();
    private readonly HashSet<GameKey> _pressedKeysLastFrame = new();
    private Vector2 _mouseDelta = Vector2.Zero;

    public MockPlayerInput()
    {
    }

    public void AddKeyPress(int frame, GameKey key)
    {
        _events.Add(new InputEvent(frame, key, true, null));
    }

    public void Update(GameWindow window)
    {
        _currentFrame++;
        
        // Store last frame's state for IsKeyPressed logic
        _pressedKeysLastFrame.Clear();
        foreach (var key in _pressedKeys)
        {
            _pressedKeysLastFrame.Add(key);
        }

        // Reset mouse delta (only applies on specific frame)
        _mouseDelta = Vector2.Zero;

        // Process all events for current frame
        var frameEvents = _events.Where(e => e.FrameNumber == _currentFrame);
        foreach (var evt in frameEvents)
        {
            if (evt.IsPressed)
            {
                _pressedKeys.Add(evt.Key);
            }
            else
            {
                _pressedKeys.Remove(evt.Key);
            }

            // Apply mouse delta if present
            if (evt.MouseDelta.HasValue)
            {
                _mouseDelta = evt.MouseDelta.Value;
            }
        }
    }

    public bool IsKeyDown(GameKey key)
    {
        return _pressedKeys.Contains(key);
    }

    public bool IsKeyPressed(GameKey key)
    {
        return _pressedKeys.Contains(key) && !_pressedKeysLastFrame.Contains(key);
    }

    public bool IsKeyPressed(Keys key)
    {
        // Not used in mock, return false
        return false;
    }

    public Vector2 GetMouseDelta()
    {
        return _mouseDelta;
    }

    public void CenterMouse(GameWindow window)
    {
        // No-op for testing
    }
}
