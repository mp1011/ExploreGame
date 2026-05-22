namespace ExploringGame.Logics;

/// <summary>
/// Common light intensity values on a scale of 0-10
/// where 0 is complete darkness, 5.0 is normal, and 10.0 is extremely bright
/// </summary>
public static class LightIntensity
{
    public const float Darkness = 0f;

    public const float VeryDim = 0.1f;

    public const float Dim = 0.4f;

    public const float IndoorLight = 1.0f;

    public const float Bright = 1.2f;

    public const float VeryBright = 2.0f;

    public const float ExtremelyBright = 3.0f;

    /// <summary>
    /// Default ambient light level for rooms without lighting data.
    /// </summary>
    public static float DefaultAmbientLight { get; set; } = Darkness;
}
