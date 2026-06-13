using ExploringGame.Services;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.GeometryBuilder.Shapes.Decals;

public class WallDecal : StampedShape<WallDecalStamp>
{
    public Side WallSide { get; set; }

    private Vector2 _centerUV;
    /// <summary>
    /// Position of the decal's center in wall-space UV coordinates.
    /// (0, 0) = center of wall, U = horizontal axis, V = vertical axis
    /// </summary>
    public Vector2 CenterUV
    {
        get => _centerUV;
        set
        {
            _centerUV = value;
            CalculateTransform();
        }
    }

    public override CollisionGroup CollisionGroup => CollisionGroup.None;
    public override CollisionGroup CollidesWithGroups => CollisionGroup.None;

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public WallDecal(Room parentRoom, Side wallSide, Vector2 centerUV)
    {
        parentRoom.AddChild(this);
        WallSide = wallSide;
        _centerUV = centerUV;
        CalculateTransform();
    }

    private void CalculateTransform()
    {
        var roomPos = Parent.LocalPosition;
        var roomSize = Parent.Size;

        // CenterUV is relative to wall center, in UV coordinates
        // U = horizontal axis of wall, V = vertical (always Y)
        Vector3 position = roomPos;
        float yaw = 0;

        switch (WallSide)
        {
            case Side.North:
                // North wall: U=X, V=Y
                position.Z = Parent.GetWorldSide(Side.North);
                position.X = roomPos.X + _centerUV.X;
                position.Y = roomPos.Y + _centerUV.Y;
                yaw = 0; // Face +Z (into room)
                break;

            case Side.South:
                // South wall: U=X (but mirrored), V=Y
                position.Z = Parent.GetWorldSide(Side.South);
                //   position.X = roomPos.X - _centerUV.X; // Mirror X for south
                // do we want to mirror, or should the signs of U,V match the original axis?
                position.X = roomPos.X + _centerUV.X; 
                position.Y = roomPos.Y + _centerUV.Y;
                yaw = (float)Math.PI; // Face -Z (into room)
                break;

            case Side.East:
                // East wall: U=Z, V=Y
                position.X = Parent.GetWorldSide(Side.East);
                position.Z = roomPos.Z + _centerUV.X;
                position.Y = roomPos.Y + _centerUV.Y;
                yaw = (float)Math.PI * 1.5f; // Face -X (into room)
                break;

            case Side.West:
                // West wall: U=Z (but mirrored), V=Y
                position.X = Parent.GetWorldSide(Side.West);
                // position.Z = roomPos.Z - _centerUV.X; // Mirror Z for west
                // see mirroring comment above
                position.Z = roomPos.Z + _centerUV.X;
                position.Y = roomPos.Y + _centerUV.Y;
                yaw = (float)Math.PI * 0.5f; // Face +X (into room)
                break;

            default:
                throw new ArgumentException($"WallDecal only supports North, South, East, West sides, not {WallSide}");
        }

        LocalPosition = position;
        Rotation = new Rotation(yaw, 0, 0);      
    }
}
