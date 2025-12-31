using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes;

class OfficeDesk : Shape
{
    public const float DeskWidth = 4.0f;
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

        AddChild(new DeskTop());
        AddChild(new DeskBottom());
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return Array.Empty<Triangle>();
    }

    abstract class DeskShape : Shape
    {
        public DeskShape(ViewFrom viewFrom)
        {
            ViewFrom = viewFrom;

            MainTexture = new TextureInfo(Key: TextureKey.Wood);
            SideTextures[Side.West] = new TextureInfo(Color: new Color(0.8f, 0.8f, 0.8f), Key: TextureKey.Wood);
        }

        public override ViewFrom ViewFrom { get; }
        protected override Triangle[] BuildInternal(QualityLevel quality)
        {
            if (Children.Any())
                return Array.Empty<Triangle>();
            else
                return BuildCuboid();
        }
    }

    class DeskTop : DeskShape
    {
        public DeskTop() : base(ViewFrom.Inside)
        {
            AddChild(new DeskUpper());
            AddChild(new DeskMiddleLeft());
            AddChild(new DeskMiddleRight());
        }

        protected override void BeforeBuild()
        {
            this.AdjustShape().From(Parent)
                              .SliceY(0, Height - DeskSurfaceHeight)
                              .SliceZ(DeskTopIndent, DeskDepth - DeskTopIndent);
        }

        protected override Triangle[] BuildInternal(QualityLevel quality)
        {
            var cuboid = BuildCuboid();
            return cuboid.Where(p => p.Side == Side.North).ToArray();
        }
    }

    class DeskBottom : DeskShape
    {
        public DeskBottom() : base(ViewFrom.Outside)
        {
            AddChild(new DeskBottomLeftDrawers());
            //   AddChild(new DeskBottomMiddleSpace());
            AddChild(new DeskBottomRight());
            AddChild(new DeskSurface());
        }

        protected override void BeforeBuild()
        {
            this.AdjustShape().From(Parent)
                              .SliceY(Height - DeskSurfaceHeight, DeskSurfaceHeight);
        }
    }

    class DeskBottomLeftDrawers : DeskShape
    {
        public DeskBottomLeftDrawers() : base(ViewFrom.Outside) { }

        protected override void BeforeBuild()
        {
            this.AdjustShape().From(Parent)
                              .SliceX(0, BottomLeftDrawerWidth)
                              .SliceY(DeskSurfaceThickness, DeskSurfaceHeight - DeskSurfaceThickness);
        }
    }

    class DeskBottomRight : DeskShape
    {
        public DeskBottomRight() : base(ViewFrom.Outside) { }

        protected override void BeforeBuild()
        {
            this.AdjustShape().From(Parent)
                              .SliceX(DeskWidth - BottomRightWidth, BottomRightWidth)
                              .SliceY(DeskSurfaceThickness, DeskSurfaceHeight - DeskSurfaceThickness);
        }
    }

    class DeskSurface : DeskShape
    {
        public DeskSurface() : base(ViewFrom.Outside) { }

        protected override void BeforeBuild()
        {
            this.AdjustShape().From(Parent)
                              .SliceY(0, DeskSurfaceThickness);

        }
    }

    /// <summary>
    /// upper drawers and cubbies
    /// </summary>
    class DeskUpper : DeskShape
    {
        public DeskUpper() : base(ViewFrom.Outside) { }

        protected override void BeforeBuild()
        {
            this.AdjustShape().From(Parent)
                              .SliceY(0, UpperHeight);

        }
    }

    class DeskMiddleLeft : DeskShape
    {
        public DeskMiddleLeft() : base(ViewFrom.Outside) { }

        protected override void BeforeBuild()
        {
            var middleHeight = DeskHeight - DeskSurfaceHeight - UpperHeight;
            this.AdjustShape().From(Parent)
                              .SliceY(UpperHeight, middleHeight)
                              .SliceX(0, MiddleSideThickness);
        }
    }

    class DeskMiddleRight : DeskShape
    {
        public DeskMiddleRight() : base(ViewFrom.Outside) { }

        protected override void BeforeBuild()
        {
            var middleHeight = DeskHeight - DeskSurfaceHeight - UpperHeight;
            this.AdjustShape().From(Parent)
                              .SliceY(UpperHeight, middleHeight)
                              .SliceX(DeskWidth - MiddleSideThickness, MiddleSideThickness);
        }
    }
}
