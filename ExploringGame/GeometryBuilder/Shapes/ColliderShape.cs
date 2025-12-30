using ExploringGame.Logics.Collision;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.GeometryBuilder.Shapes;

/// <summary>
/// non-rendered shape that helps with collision
/// </summary>
public abstract class ColliderShape : Shape
{
    public static bool DisplayColliders = false;
    public override bool CollisionEnabled => true;

    public override ViewFrom ViewFrom => DisplayColliders ? ViewFrom.Outside : ViewFrom.None;

    public ColliderShape()
    {
        MainTexture = new Texture.TextureInfo(new Color(0,255,0,100));
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        if (DisplayColliders)
            return BuildCuboid();
        else
            return Array.Empty<Triangle>();
    }
}

public class RoomJoiner : ColliderShape, ICollisionResponder
{
    public const float Padding = 0.4f;

    public RoomJoiner(Room room1, Room room2, RoomConnection connection)
    {
        Room1 = room1;
        Room2 = room2;
        Connection = connection;

        // temp, need to calc which room is "thinner"        
        this.AdjustShape().From(Room2);

        var pad = connection.Side switch
        {
            Side.North => Padding,
            Side.West => Padding,
            Side.South => -Padding,
            Side.East => -Padding,
            _ => 0
        };

        SetSide(connection.Side.Opposite(), room2.GetSide(connection.Side.Opposite()) + pad);
        SetSideUnanchored(connection.Side, room2.GetSide(connection.Side.Opposite()) - pad);
    }

    public Room Room1 { get; }
    public Room Room2 { get; }
    public RoomConnection Connection { get; }

    public CollisionResponse CheckCollision(CollisionResponse lastResponse)
    {
        if (!ContainsPoint(lastResponse.NewPosition))
            return lastResponse;

        return new CollisionResponse(lastResponse.OriginalPosition, lastResponse.NewPosition, IgnoreWallCollision: true);
    }
}
