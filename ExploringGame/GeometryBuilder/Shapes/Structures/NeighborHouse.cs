using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.Services;
using ExploringGame.Texture;
using Jitter2.Collision.Shapes;
using Microsoft.Xna.Framework;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes.Structures;

public class NeighborHouse : Shape
{
    public override Theme Theme { get; }

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public NeighborHouse()
    {
        Theme = new Theme(Color.Black);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        var northPart = new Box {  Size = Size, LocalPosition = LocalPosition };
        northPart.SetSideUnanchored(Side.South, LocalPosition.Z);

        var southPart = new Box { Size = Size, LocalPosition = LocalPosition };
        southPart.SetSideUnanchored(Side.North, LocalPosition.Z);

        var northTriangles = TriangleMaker.BuildCuboid(northPart);
        var southTriangles = TriangleMaker.BuildCuboid(southPart);


        northTriangles = new VertexOffsetter().Execute(northPart, northTriangles, new VertexOffset(Side.South | Side.Top, new Vector3(0, Measure.Feet(10), 0)));
        southTriangles = new VertexOffsetter().Execute(southPart, southTriangles, new VertexOffset(Side.North | Side.Top, new Vector3(0, Measure.Feet(10),0)));

        return northTriangles.Union(southTriangles).ToArray();
    }
}
