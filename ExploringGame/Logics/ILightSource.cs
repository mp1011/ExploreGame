using ExploringGame.GeometryBuilder.Shapes;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.Logics;

/// <summary>
/// Represents a light source that contributes to room lighting calculations
/// </summary>
public interface ILightSource
{
    /// <summary>
    /// Light intensity on a scale of 0-10 (5.0 = normal, 10.0 = extremely bright)
    /// </summary>
    float Intensity { get; set; }

    /// <summary>
    /// Color of the light
    /// </summary>
    Color Color { get; set; }

    /// <summary>
    /// Position of the light in world space
    /// </summary>
    Vector3 LightPosition { get; }

    /// <summary>
    /// Whether the light is currently on
    /// </summary>
    bool On { get; set; }

    Room Room { get; }

    /// <summary>
    /// Raised when the light's on/off state changes
    /// </summary>
    event EventHandler<LightStateChangedEventArgs> StateChanged;
}

public class LightStateChangedEventArgs : EventArgs
{
    public bool IsOn { get; }

    public LightStateChangedEventArgs(bool isOn)
    {
        IsOn = isOn;
    }
}
