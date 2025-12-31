using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes;

class OfficeDesk : Shape
{
    public const float DeskWidth = 3.0f;
    public const float DeskHeight = 2.2f;
    public const float DeskDepth = 1.0f;

    public const float DeskSurfaceHeight = 1.0f;
    public const float DeskSurfaceThickness = 0.1f;

    public const float DeskTopIndent = 0.5f;

    public const float BottomLeftDrawerWidth = 1.0f;
    public const float BottomRightWidth = 0.1f;

    public const float MiddleSideThickness = 0.1f;
    public const float UpperHeight = 0.6f;

    public override bool CollisionEnabled => true;

    public override ViewFrom ViewFrom => ViewFrom.Outside;


    public OfficeDesk(Shape room)
    {
        room.AddChild(this);

        Width = DeskWidth;
        Height = DeskHeight;
        Depth = DeskDepth;

        MainTexture = new TextureInfo(Key: TextureKey.Wood);
        SideTextures[Side.West] = new TextureInfo(Color: new Color(0.8f, 0.8f, 0.8f), Key: TextureKey.Wood);

        var middleHeight = DeskHeight - DeskSurfaceHeight - UpperHeight;

        var builder = new ShapeBuilder();
        var top = builder.AddChild(this, new DeskTopPart(adj => adj.SliceY(0, Height - DeskSurfaceHeight)
                                                   .SliceZ(DeskTopIndent, DeskDepth - DeskTopIndent)));

        var upper = builder.AddChild(top, adj => adj.SliceY(0, UpperHeight));

        // middle left
        builder.AddChild(top, adj => adj.SliceY(UpperHeight, middleHeight)
                                        .SliceX(0, MiddleSideThickness));
        // middle right
        builder.AddChild(top, adj => adj.SliceY(UpperHeight, middleHeight)
                                        .SliceX(DeskWidth - MiddleSideThickness, MiddleSideThickness));

        var bottom = builder.AddChild(this, adj => adj.SliceY(Height - DeskSurfaceHeight, DeskSurfaceHeight));

        // lower left drawers 
        builder.AddChild(bottom, adj => adj.SliceX(0, BottomLeftDrawerWidth)
                                           .SliceY(DeskSurfaceThickness, DeskSurfaceHeight - DeskSurfaceThickness));
            
        // bottom right 
        builder.AddChild(bottom, adj => adj.SliceX(DeskWidth - BottomRightWidth, BottomRightWidth)
                                           .SliceY(DeskSurfaceThickness, DeskSurfaceHeight - DeskSurfaceThickness));

        // surface
        builder.AddChild(bottom, adj => adj.SliceY(0, DeskSurfaceThickness));
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return Array.Empty<Triangle>();
    }

    class DeskTopPart : ComplexShapePart
    {
        public DeskTopPart(Action<ShapeAdjuster> adjust) : base(ViewFrom.Inside, adjust)
        {
        }

        protected override Triangle[] BuildInternal(QualityLevel quality)
        {
            return BuildCuboid().Where(p=>p.Side == Side.North).ToArray();
        }
    }
}
