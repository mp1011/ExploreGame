
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes;

public abstract class ShapePart : Shape
{
    private Vector3 _originalPos;

    protected sealed override void BeforeBuild()
    {
        _originalPos = Position;

        Position += Parent.Position;        
    }

    protected sealed override void AfterBuild()
    {
        Position = _originalPos;
    }
}
