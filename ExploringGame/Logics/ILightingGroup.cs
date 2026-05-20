using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using System;

namespace ExploringGame.Logics;

/// <summary>
/// Interface for objects that can provide ambient lighting information
/// </summary>
public interface ILightingGroup : IRoom
{
    string Tag { get; }
}

public class DefaultLightingGroup : ILightingGroup
{
    public static DefaultLightingGroup Instance { get; } = new DefaultLightingGroup();

    public string Tag => "[Default]";

    public Shape[] TraverseAllChildren() => Array.Empty<Shape>();
}
