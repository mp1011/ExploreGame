using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes;

public abstract class SkyboxShape : Shape, ILightingGroup
{
    public override ViewFrom ViewFrom => ViewFrom.Inside;

    public SkyboxShape()
    {
        Position = Vector3.Zero;
        Size = new Vector3(50f, 50f, 50f);
    }

    public override Matrix GetWorldMatrix()
    {
        return Matrix.Identity;
    }
}
