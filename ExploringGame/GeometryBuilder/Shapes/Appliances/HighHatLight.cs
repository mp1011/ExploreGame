using ExploringGame.Services;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Appliances;

public class HighHatLight : Shape, ICutoutShape
{
    public override Theme Theme => new Theme(TextureKey.Ceiling);
    public override ViewFrom ViewFrom => ViewFrom.Inside;

    public Side ParentCutoutSide => Side.Top;

    Triangle[] ICutoutShape.Build() => BuildInternal(QualityLevel.Basic);

    public HighHatLight(Room room)
    {
        room.AddChild(this);

        Height = 0.1f;
        Width = 0.5f;
        Depth = 0.5f;

        this.Place().OnSideOuter(Side.Top, room);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return TriangleMaker.BuildCylinder(this, detail: 4, Axis.Y);
    }
}
