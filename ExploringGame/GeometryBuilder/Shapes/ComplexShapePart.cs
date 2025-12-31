using ExploringGame.Services;
using System;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes;

public class ComplexShapePart : Shape
{
    private Action<ShapeAdjuster> _adjust;

    public ComplexShapePart(ViewFrom viewFrom, Action<ShapeAdjuster> adjust)
    {
        ViewFrom = viewFrom;
        _adjust = adjust;
    }

    public override ViewFrom ViewFrom { get; }

    protected override void BeforeBuild()
    {
        _adjust(this.AdjustShape().From(Parent));
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        if (Children.Any())
            return Array.Empty<Triangle>();
        else
            return BuildCuboid();
    }    
}
