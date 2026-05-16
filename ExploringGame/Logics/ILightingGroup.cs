namespace ExploringGame.Logics;

/// <summary>
/// Interface for objects that can provide ambient lighting information
/// </summary>
public interface ILightingGroup
{
    string Tag { get; }
}

public class DefaultLightingGroup : ILightingGroup
{
    public static DefaultLightingGroup Instance { get; } = new DefaultLightingGroup();

    public string Tag => "[Default]";
}
