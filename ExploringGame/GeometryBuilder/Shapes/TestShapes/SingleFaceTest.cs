using Microsoft.Xna.Framework;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes.TestShapes;

public class SingleFaceTest : Shape
{
    private Side _side;

    public SingleFaceTest(Side side)
    {
        MainColor = Color.Transparent;
        SideColors[side] = Color.White;

        Width = 1.0f;
        Height = 1.0f;
        Depth = 1.0f;
    }
    public override ViewFrom ViewFrom => ViewFrom.Outside;


    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid().Where(p => p.Color != Color.Transparent).ToArray();
    }
}
