using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes.Furniture;

class OfficeDesk : PlaceableShape
{
    public static readonly float DeskWidth = Measure.Inches(71);
    public static readonly float DeskHeight = Measure.Inches(65);
    public static readonly float DeskDepth = Measure.Inches(24);

    public static readonly float DeskSurfaceHeight = 1.0f;
    public static readonly float DeskSurfaceThickness = 0.1f;

    public static readonly float DeskTopIndent = 0.5f;

    public static readonly float BottomLeftDrawerWidth = 1.0f;
    public static readonly float BottomRightWidth = 0.1f;

    public static readonly float MiddleSideThickness = 0.1f;
    public static readonly float UpperHeight = 0.6f;

    public override IColliderMaker ColliderMaker => ColliderMakers.BoundingBox(this);

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
            return BuildCuboid().Where(p=>p.Side == Side.South).ToArray();
        }
    }
}
