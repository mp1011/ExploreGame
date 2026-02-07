using ExploringGame.GeometryBuilder;
using ExploringGame.Logics.Collision;
using Microsoft.Xna.Framework;

namespace ExploringGame.Entities;

public interface IWithPosition
{
    public Vector3 Position { get; set; }
    public Vector3 Size { get; }
    public Rotation Rotation { get; set; }
}

public static class IWithPositionExtensions
{
    public static Vector2 TopDownPosition(this IWithPosition entity) => new Vector2(entity.Position.X, entity.Position.Z);
    public static float Width(this IWithPosition p) => p.Size.X;
    public static float Height(this IWithPosition p) => p.Size.Y;
    public static float Depth(this IWithPosition p) => p.Size.Z;
}