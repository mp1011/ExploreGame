using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes;

class SurfaceIndent : Shape
{
    private Side _face;
    private Placement2D _placement;
    private float _depth;

    public override ViewFrom ViewFrom => ViewFrom.Inside;

    public SurfaceIndent(Shape parent, Side face, Placement2D placement, float depth)
    {
        MainTexture = new TextureInfo(Texture.TextureKey.Wood);

        _face = face;
        _placement = placement;
        _depth = depth;
        parent.AddChild(this);
    }

    protected override void BeforeBuild()
    {       
        this.AdjustShape().From(this.Parent);

        if(_face == Side.North || _face == Side.South)
            Depth = _depth;
        else 
            Width = _depth;
     
        this.Place().OnSideInner(_face);
        
        this.AdjustShape().WithInnerOffset(_placement, _face);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }
}
