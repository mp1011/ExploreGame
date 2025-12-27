using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ExploringGame.Extensions;

public static class VectorExtensions
{

    public static Vector3 Center(this IEnumerable<Vector3> points)
    {
        if (points == null || !points.Any())
            return Vector3.Zero;

        Vector3 sum = Vector3.Zero;

        foreach (var v in points)
            sum += v;

        return sum / points.Count();
    }

    public static Vector2 Project(this Vector3 p, Vector3 O, Vector3 U, Vector3 V)
    {
        Vector3 d = p - O;
        return new Vector2(
            Vector3.Dot(d, U),
            Vector3.Dot(d, V)
        );
    }

    public static Vector3 Unproject(this Vector2 p, Vector3 O, Vector3 U, Vector3 V)
    {
        return O + p.X * U + p.Y * V;
    }
        
    public static Vector2 Set(this Vector2 vector, Axis axis, float newValue)
    {
        return axis switch
        {
            Axis.X => new Vector2(newValue, vector.Y),
            Axis.Y => new Vector2(vector.X, newValue),
            _ => throw new InvalidEnumArgumentException("Invalid Axis for 2D Vector")
        };
    }

    public static Vector2 Offset(this Vector2 vector, float offsetX, float offsetY)
    {
        return new Vector2(vector.X + offsetX, vector.Y + offsetY);
    }

    public static double AngleTo(this Vector2 p1, Vector2 p2)
    {
        double deltaY = p2.Y - p1.Y;
        double deltaX = p2.X - p1.X;
        double radians = Math.Atan2(deltaY, deltaX);
        double degrees = radians * (180.0 / Math.PI);

        return degrees.NMod(360.0);
    }

    public static double AngleBetween(this Vector2 p1, Vector2 p2, Vector2 p3)
    {
        var angle1 = p1.AngleTo(p2);
        var angle2 = p1.AngleTo(p3);

        var angleBetween1 = (angle1 - angle2).NMod(360.0);
        var angleBetween2 = (angle2 - angle1).NMod(360.0);

        return Math.Min(angleBetween1, angleBetween2);
    }

    public static Vector2 MoveToward(this Vector2 point, Vector2 target, float delta)
    {
        if (point == target || delta <= 0)
            return point;
      
        // handle axis aligned separately to avoid rounding errors
        if (point.X == target.X)
        {
            if (point.Y < target.Y)
                return new Vector2(point.X, point.Y + delta);
            else
                return new Vector2(point.X, point.Y - delta);
        }

        if (point.Y == target.Y)
        {
            if (point.X < target.X)
                return new Vector2(point.X + delta, point.Y);
            else
                return new Vector2(point.X - delta, point.Y);
        }
        

        double angle = Math.Atan2(target.Y - point.Y, target.X - point.X);
        return new Vector2(
            (float)(point.X + delta * Math.Cos(angle)),
            (float)(point.Y + delta * Math.Sin(angle))
        );
    }
}
