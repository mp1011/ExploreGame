namespace ExploringGame.Logics;

/// <summary>
/// Common light intensity values on a scale of 0-10
/// where 0 is complete darkness, 5.0 is normal, and 10.0 is extremely bright
/// </summary>
public static class LightIntensity
{
    /// <summary>
    /// Complete darkness (0.0)
    /// </summary>
    public const float Darkness = 0f;

    /// <summary>
    /// Very dim light (1.0)
    /// </summary>
    public const float VeryDim = 1f;

    /// <summary>
    /// Dim light (2.0)
    /// </summary>
    public const float Dim = 2f;

    /// <summary>
    /// Indoor lighting - typical for residential rooms (3.0)
    /// </summary>
    public const float IndoorLight = 3f;

    /// <summary>
    /// Normal light level (5.0)
    /// </summary>
    public const float Normal = 5f;

    /// <summary>
    /// Bright light (7.0)
    /// </summary>
    public const float Bright = 7f;

    /// <summary>
    /// Very bright light (9.0)
    /// </summary>
    public const float VeryBright = 9f;

    /// <summary>
    /// Extremely bright - blindingly bright (10.0)
    /// </summary>
    public const float ExtremelyBright = 10f;
}
