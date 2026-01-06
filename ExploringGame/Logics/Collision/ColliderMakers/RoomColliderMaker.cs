using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Services;
using Jitter2.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ExploringGame.Logics.Collision.ColliderMakers;


public class RoomColliderMaker : IColliderMaker
{
    public static bool DebugDisplaySideColliders = false;

    private Room _room;

    public RoomColliderMaker(Room room)
    {
        _room = room;
    }

    public IEnumerable<RigidBody> CreateColliders(Physics physics)
    {
        AddColliders(physics, Side.West);
        AddColliders(physics, Side.East);
        AddColliders(physics, Side.North);
        AddColliders(physics, Side.South);
        AddColliders(physics, Side.Top);
        AddColliders(physics, Side.Bottom);

        // todo
        return Array.Empty<RigidBody>();
    }

    private void AddColliders(Physics physics, Side side)
    {
        var roomConnections = _room.GetRoomConnections(side);
        if (roomConnections.Length == 0)
        {
            physics.CreateStaticSurface(_room, side);
            return;
        }

        if (roomConnections.Length > 1)
            throw new System.NotImplementedException("not yet working for multiple connections on same side");

        var placement = roomConnections[0].CalcCutoutPlacement(_room);

        var sideLength = _room.SideLength(side);

        var shapeCopyL = new Box { Position = _room.Position, Size = _room.Size, Rotation = _room.Rotation };
        var shapeCopyR = new Box { Position = _room.Position, Size = _room.Size, Rotation = _room.Rotation };

        switch (side)
        {
            case Side.North:
                shapeCopyL.AdjustShape().SliceX(0f, placement.Left);
                shapeCopyL.SetSide(Side.North, _room.GetSide(Side.North) - Physics.WallColliderThickness);
                shapeCopyL.SetSideUnanchored(Side.South, _room.GetSide(Side.North));

                shapeCopyR.AdjustShape().SliceX(sideLength - placement.Right, placement.Right);
                shapeCopyR.SetSide(Side.North, _room.GetSide(Side.North) - Physics.WallColliderThickness);
                shapeCopyR.SetSideUnanchored(Side.South, _room.GetSide(Side.North));
                break;
            case Side.South:
                shapeCopyL.AdjustShape().SliceX(sideLength - placement.Left, placement.Left);
                shapeCopyL.SetSide(Side.South, _room.GetSide(Side.South) + Physics.WallColliderThickness);
                shapeCopyL.SetSideUnanchored(Side.North, _room.GetSide(Side.South));

                shapeCopyR.AdjustShape().SliceX(0f, placement.Right);
                shapeCopyR.SetSide(Side.South, _room.GetSide(Side.South) + Physics.WallColliderThickness);
                shapeCopyR.SetSideUnanchored(Side.North, _room.GetSide(Side.South));
                break;
            case Side.West:
                shapeCopyL.AdjustShape().SliceZ(sideLength - placement.Left, placement.Left);
                shapeCopyL.SetSide(Side.West, _room.GetSide(Side.West) - Physics.WallColliderThickness);
                shapeCopyL.SetSideUnanchored(Side.East, _room.GetSide(Side.West));

                shapeCopyR.AdjustShape().SliceZ(0f, placement.Right);
                shapeCopyR.SetSide(Side.West, _room.GetSide(Side.West) - Physics.WallColliderThickness);
                shapeCopyR.SetSideUnanchored(Side.East, _room.GetSide(Side.West));
                break;
            case Side.East:
                shapeCopyL.AdjustShape().SliceZ(0f, placement.Left);
                shapeCopyL.SetSide(Side.West, _room.GetSide(Side.East));
                shapeCopyL.SetSideUnanchored(Side.East, _room.GetSide(Side.East) + Physics.WallColliderThickness);

                shapeCopyR.AdjustShape().SliceZ(sideLength - placement.Right, placement.Right);
                shapeCopyR.SetSide(Side.West, _room.GetSide(Side.East));
                shapeCopyR.SetSideUnanchored(Side.East, _room.GetSide(Side.East) + Physics.WallColliderThickness);
                break;
            case Side.Top:
                throw new System.Exception("fix me");
            case Side.Bottom:
                throw new System.Exception("fix me");
            default:
                throw new System.Exception("invalid side");

        }

        if (DebugDisplaySideColliders)
        {
            shapeCopyL.MainTexture = new Texture.TextureInfo(Color.Blue);
            _room.AddChild(shapeCopyL);

            shapeCopyR.MainTexture = new Texture.TextureInfo(Color.Red);
            _room.AddChild(shapeCopyR);
        }

        if(shapeCopyL.Size.IsValidPositive())
            physics.CreateStaticBody(shapeCopyL);

        if (shapeCopyR.Size.IsValidPositive())
            physics.CreateStaticBody(shapeCopyR);
    }

}
