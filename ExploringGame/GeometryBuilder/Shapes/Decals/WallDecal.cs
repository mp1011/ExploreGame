using ExploringGame.Services;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.GeometryBuilder.Shapes.Decals;

public class WallDecal : StampedShape<WallDecalStamp>
{
    public Side WallSide { get; set; }
    public Placement2D Placement { get; set; }

    public override CollisionGroup CollisionGroup => CollisionGroup.None;
    public override CollisionGroup CollidesWithGroups => CollisionGroup.None;

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public WallDecal(Room parentRoom, Side wallSide, Placement2D placement)
    {
        WallSide = wallSide;
        Placement = placement;

        // Calculate position and rotation based on wall side
        CalculateTransform(parentRoom);
    }

    private void CalculateTransform(Room parentRoom)
    {
        var roomPos = parentRoom.Position;
        var roomSize = parentRoom.Size;

        // Calculate dimensions from Placement
        float width = Placement.Right - Placement.Left;
        float height = Placement.Top - Placement.Bottom;
        float centerLeft = Placement.Left + width / 2;
        float centerBottom = Placement.Bottom + height / 2;

        // Start with decal position at room center
        Vector3 position = roomPos;
        float yaw = 0;

        switch (WallSide)
        {
            case Side.North:
                // North wall is at -Z
                position.Z = parentRoom.GetSide(Side.North);
                position.X = roomPos.X - (roomSize.X / 2) + centerLeft;
                position.Y = parentRoom.GetSide(Side.Bottom) + centerBottom;
                yaw = 0; // Face +Z (into room)
                break;

            case Side.South:
                // South wall is at +Z
                position.Z = parentRoom.GetSide(Side.South);
                position.X = roomPos.X + (roomSize.X / 2) - centerLeft;
                position.Y = parentRoom.GetSide(Side.Bottom) + centerBottom;
                yaw = (float)Math.PI; // Face -Z (into room)
                break;

            case Side.East:
                // East wall is at +X
                position.X = parentRoom.GetSide(Side.East);
                position.Z = roomPos.Z - (roomSize.Z / 2) + centerLeft;
                position.Y = parentRoom.GetSide(Side.Bottom) + centerBottom;
                yaw = (float)Math.PI * 1.5f; // Face -X (into room)
                break;

            case Side.West:
                // West wall is at -X
                position.X = parentRoom.GetSide(Side.West);
                position.Z = roomPos.Z + (roomSize.Z / 2) - centerLeft;
                position.Y = parentRoom.GetSide(Side.Bottom) + centerBottom;
                yaw = (float)Math.PI * 0.5f; // Face +X (into room)
                break;

            default:
                throw new ArgumentException($"WallDecal only supports North, South, East, West sides, not {WallSide}");
        }

        Position = position;
        Rotation = new Rotation(yaw, 0, 0);
        
        // Scale to match placement size
        Width = width;
        Height = height;
        Depth = 0.01f;
    }
}
