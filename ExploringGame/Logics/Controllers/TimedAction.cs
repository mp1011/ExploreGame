using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.Logics.Controllers;

/// <summary>
/// Encapsulates an action that should be executed at regular intervals
/// </summary>
public class TimedAction
{
    private TimeSpan _elapsed;
    private readonly TimeSpan _interval;
    private readonly Action _action;
    private readonly bool _autoReset;

    public TimeSpan Elapsed => _elapsed;
    public TimeSpan Interval => _interval;
    public bool IsReady => _elapsed >= _interval;

    /// <summary>
    /// Creates a timed action
    /// </summary>
    /// <param name="interval">Time between executions</param>
    /// <param name="action">Action to execute when interval is reached</param>
    /// <param name="autoReset">Whether to automatically reset after execution</param>
    public TimedAction(TimeSpan interval, Action action, bool autoReset = true)
    {
        _interval = interval;
        _action = action;
        _autoReset = autoReset;
        _elapsed = TimeSpan.Zero;
    }

    /// <summary>
    /// Updates the timer and executes action if interval is reached
    /// </summary>
    public void Update(GameTime gameTime)
    {
        _elapsed += gameTime.ElapsedGameTime;

        if (IsReady)
        {
            _action?.Invoke();
            
            if (_autoReset)
            {
                _elapsed = TimeSpan.Zero;
            }
        }
    }

    /// <summary>
    /// Manually resets the timer
    /// </summary>
    public void Reset()
    {
        _elapsed = TimeSpan.Zero;
    }
}

