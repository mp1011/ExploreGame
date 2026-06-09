using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ExploringGame.GeometryBuilder.Shapes;

public abstract class SkyboxShape : Shape, ILightingGroup
{
    public override ViewFrom ViewFrom => ViewFrom.Inside;

    public IEnumerable<RoomConnection> RoomConnections => Array.Empty<RoomConnection>();

    public WorldSegment WorldSegment => null;

    public SkyboxShape()
    {
        LocalPosition = Vector3.Zero;
        Size = new Vector3(50f, 50f, 50f);
    }

    public override Matrix GetWorldMatrix()
    {
        return Matrix.Identity;
    }
}
