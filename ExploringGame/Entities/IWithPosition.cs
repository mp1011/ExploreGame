using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using ExploringGame.Logics.Collision;
using Microsoft.Xna.Framework;
using System;
using System.Drawing;

namespace ExploringGame.Entities;

public interface IWithPosition
{
    public Vector3 WorldPosition {  get; set; }
    public Vector3 Size { get; set; }
    public Rotation Rotation { get; set; }
}

public static class IWithPositionExtensions
{
    public static Vector2 TopDownPosition(this IWithPosition entity) => new Vector2(entity.WorldPosition.X, entity.WorldPosition.Z);
    public static float Width(this IWithPosition p) => p.Size.X;
    public static float Height(this IWithPosition p) => p.Size.Y;
    public static float Depth(this IWithPosition p) => p.Size.Z;

    public static float GetWorldSide(this IWithPosition entity, Side side)
    {
        return side switch
        {
            Side.North => entity.WorldPosition.Z - entity.Size.Z / 2f,
            Side.South => entity.WorldPosition.Z + entity.Size.Z / 2f,
            Side.West => entity.WorldPosition.X - entity.Size.X / 2f,
            Side.East => entity.WorldPosition.X + entity.Size.X / 2f,
            Side.Top => entity.WorldPosition.Y + entity.Size.Y / 2f,
            Side.Bottom => entity.WorldPosition.Y - entity.Size.Y / 2f,
            _ => throw new ArgumentException("Only singular sides can be used")
        };
    }

    public static void SetWorldSide(this IWithPosition entity, Side side, float value)
    {
        switch (side)
        {
            case Side.North:
                entity.WorldPosition = entity.WorldPosition.SetZ(value + entity.Size.Z / 2f);
                return;
            case Side.South:
                entity.WorldPosition = entity.WorldPosition.SetZ(value - entity.Size.Z / 2f);
                return;
            case Side.West:
                entity.WorldPosition = entity.WorldPosition.SetX(value + entity.Size.X / 2f);
                return;
            case Side.East:
                entity.WorldPosition = entity.WorldPosition.SetX(value - entity.Size.X / 2f);
                return;
            case Side.Top:
                entity.WorldPosition = entity.WorldPosition.SetY(value - entity.Size.Y / 2f);
                return;
            case Side.Bottom:
                entity.WorldPosition = entity.WorldPosition.SetY(value + entity.Size.Y / 2f);
                return;
            default:
                throw new ArgumentException("Only singular sides can be used");
        }
    }


    public static void SetWorldSideUnanchored(this IWithPosition entity, Side side, float value)
    {
        var currentOpposite = entity.GetWorldSide(side.Opposite());
        entity.SetWorldSide(side, value);

        var oppDelta = entity.GetWorldSide(side.Opposite()) - currentOpposite;

        var depth = entity.Depth();
        var width = entity.Width();
        var height = entity.Height();

        switch (side)
        {
            case Side.North:
                depth -= oppDelta;
                break;
            case Side.South:
                depth += oppDelta;
                break;
            case Side.West:
                width -= oppDelta;
                break;
            case Side.East:
                width += oppDelta;
                break;
            case Side.Top:
                height += oppDelta;
                break;
            case Side.Bottom:
                height -= oppDelta;
                break;
            default:
                throw new System.ArgumentException("invalid side");
        }

        entity.Size = new Vector3(width, height, depth);

        entity.SetWorldSide(side, value);
    }

}