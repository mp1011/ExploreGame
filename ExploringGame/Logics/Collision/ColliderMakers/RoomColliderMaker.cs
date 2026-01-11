using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Services;
using Jitter2.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
        var triangles = _room.Build(QualityLevel.Basic)[_room];

        Side[] sides = new Side[]
        {
            Side.North,
            Side.South,
            Side.East,
            Side.West,
            Side.Top,
            Side.Bottom,
        };

        return sides.Select(p => physics.CreateMeshShape(triangles.Where(t => t.Side == p).ToArray()));
    }
}
