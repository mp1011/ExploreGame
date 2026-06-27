using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ExploringGame.Logics;

/// <summary>
/// Interface for objects that can provide ambient lighting information
/// </summary>
public interface ILightingGroup : IRoom
{
}

public class DefaultLightingGroup : ILightingGroup
{
    public static DefaultLightingGroup Instance { get; } = new DefaultLightingGroup();

    public string Tag => "[Default]";

    public ILightingGroup LightingGroup => this;

    public Vector3 LocalPosition => Vector3.Zero;

    public IEnumerable<RoomConnection> RoomConnections => Array.Empty<RoomConnection>();

    public WorldSegment WorldSegment => null;

    public Vector3 WorldPosition { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public Vector3 Size => throw new NotImplementedException();

    public Rotation Rotation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public bool ContainsPoint(Vector3 point) => false;

    public float SideLength(Side side) => 0;

    public Shape[] TraverseAllChildren() => Array.Empty<Shape>();
}
