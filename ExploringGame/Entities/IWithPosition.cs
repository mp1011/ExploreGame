using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;

namespace ExploringGame.Entities;

public interface IWithPosition
{
    public Vector3 Position { get; set; }
    public Rotation Rotation { get; set; }
}

public static class IWithPositionExtensions
{
    public static Vector2 TopDownPosition(this IWithPosition entity) => new Vector2(entity.Position.X, entity.Position.Z);
}