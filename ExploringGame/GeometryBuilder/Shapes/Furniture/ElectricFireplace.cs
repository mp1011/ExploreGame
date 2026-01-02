using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.GeometryBuilder.Shapes.Furniture;

/// <summary>
/// FireplaceTop
/// FireplaceBottom
///     FirePlaceMiddleShelf
///     FirePlaceLower
/// </summary>
class ElectricFireplace : PlaceableShape
{
    public const float TopShelfHeight = 0.1f;
    public const float TopShelfOverhang = 0.05f;
    public const float MiddleShelfHeight = 0.3f;
    
    public const float MiddleShelfSide = 0.1f;
    public const float MiddleShelfTop = 0.1f;
    public const float MiddleShelfBottom = 0.1f;

    public static readonly float MainWidth = Measure.Inches(60);
    public static readonly float MainHeight = Measure.Inches(34);
    public static readonly float MainDepth = Measure.Inches(16);

    public const float LowerDoorWidth = 0.6f;
    public const float LowerDrawerSideThickness = 0.1f;
    public const float LowerDoorThickness = 0.1f;

    // 60

    public const float LowerDoorWindowWidth = 0.2f;
    public const float LowerDoorWindowHeight = 0.15f;
    public const float LowerDoorWindowXSpacing = 0.01f;
    public const float LowerDoorWindowYSpacing = 0.01f;
    public const float LowerDoorWindowIndent = 0.01f;

    public const float HeatingUnitBorderSide = 0.06f;
    public const float HeatingUnitBorderTop = 0.1f;
    public const float HeatingUnitInset = 0.1f;

    public const float FooterHeight = 0.1f;

    public override bool CollisionEnabled => true;
    public override ViewFrom ViewFrom =>  ViewFrom.Outside;

    public ElectricFireplace(Shape parent)
    {
        parent.AddChild(this);
        Width = MainWidth;
        Depth = MainDepth;
        Height = MainHeight;
        MainTexture = new TextureInfo(TextureKey.Wood);

        AddChild(new FireplaceTop(this));
        AddChild(new FireplaceBottom(this));
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return Array.Empty<Triangle>();
    }

    /// <summary>
    /// top shelf
    /// </summary>
    public class FireplaceTop : Shape
    {
        public override ViewFrom ViewFrom => ViewFrom.Outside;

        public FireplaceTop(ElectricFireplace firePlace)
        {
            MainTexture = new TextureInfo(TextureKey.Wood);
        }

        protected override Triangle[] BuildInternal(QualityLevel quality)
        {
            this.AdjustShape().From(Parent)
                         .SliceY(fromTop: 0.0f, height: TopShelfHeight)
                         .AxisStretch(Axis.X | Axis.Z, TopShelfOverhang);
            return BuildCuboid();
        }

    }

    /// <summary>
    /// base, drawers, fireplace, and middle shelves
    /// </summary>
    public class FireplaceBottom : Shape
    {
        public override ViewFrom ViewFrom => ViewFrom.Outside;

        public FireplaceBottom(ElectricFireplace firePlace)
        {
            MainTexture = new TextureInfo(TextureKey.Wood);

            AddChild(new FirePlaceMiddleShelf(this));
            AddChild(new FireplaceLower(this));
        }
        protected override Triangle[] BuildInternal(QualityLevel quality)
        {
            this.AdjustShape().From(Parent)
                         .SliceY(fromTop: TopShelfHeight, height: Parent.Height - TopShelfHeight);

            return Array.Empty<Triangle>();
        }
    }


    public class FirePlaceMiddleShelf : Shape
    {
        private Placement2D _innerShelfPlacement = new Placement2D(Left: MiddleShelfSide, Right: MiddleShelfSide,
                Bottom: MiddleShelfBottom, Top: MiddleShelfTop);
        public override ViewFrom ViewFrom => ViewFrom.Outside;

        public FirePlaceMiddleShelf(FireplaceBottom firePlace)
        {
            MainTexture = new TextureInfo(TextureKey.Wood);

            AddChild(new SurfaceIndent(this, Side.South, _innerShelfPlacement, MainDepth - 0.1f));
        }

        protected override void BeforeBuild()
        {
            base.BeforeBuild();
        }

        protected override Triangle[] BuildInternal(QualityLevel quality)
        {
            this.AdjustShape().From(Parent)
                         .SliceY(fromTop: 0, height: MiddleShelfHeight);

            // thought, maybe shape building should be its own service?
            var cuboid = BuildCuboid();
            return new RemoveSurfaceRegion().Execute(cuboid, Side.South, _innerShelfPlacement);
        }
    }

    /// <summary>
    /// side drawers, heating unit, footer
    /// </summary>
    public class FireplaceLower : Shape
    {
        public override ViewFrom ViewFrom => ViewFrom.Outside;

        public FireplaceLower(FireplaceBottom firePlace)
        {
            MainTexture = new TextureInfo(TextureKey.Wood);

            AddChild(new FireplaceLowerDrawer(Side.West));
            AddChild(new FireplaceLowerDrawer(Side.East));
            AddChild(new FireplaceHeatingUnitBorder());
            AddChild(new FireplaceFooter());

        }

        protected override Triangle[] BuildInternal(QualityLevel quality)
        {
            this.AdjustShape().From(Parent)
                         .SliceY(fromTop: MiddleShelfHeight, height: Parent.Height - MiddleShelfHeight);
            return Array.Empty<Triangle>();
        }
    }

    /// <summary>
    /// Door, DrawerSpace
    /// </summary>
    public class FireplaceLowerDrawer : Shape
    {
        private Placement2D _innerSpacePlacement;
        public Side Side { get; }

        public FireplaceLowerDrawer(Side side)
        {
            Side = side;
            MainTexture = new TextureInfo(TextureKey.Wood);

            _innerSpacePlacement = new Placement2D(LowerDrawerSideThickness, LowerDrawerSideThickness, LowerDrawerSideThickness, LowerDrawerSideThickness);

            AddChild(new SurfaceIndent(this, Side.South, _innerSpacePlacement, MainDepth - LowerDoorThickness*2));
            AddChild(new FireplaceDrawerDoor(side));
        }

        public override ViewFrom ViewFrom => ViewFrom.Outside;

        protected override Triangle[] BuildInternal(QualityLevel quality)
        {
            var fromWest = 0f;
            if (Side == Side.East)
                fromWest = MainWidth - LowerDoorWidth;

            this.AdjustShape().From(Parent)
                              .SliceX(fromWest, LowerDoorWidth)
                              .SliceZ(LowerDoorThickness, MainDepth - LowerDoorThickness);

            var cuboid = BuildCuboid();
            return new RemoveSurfaceRegion().Execute(cuboid, Side.South, _innerSpacePlacement);
        }
    }

    public class FireplaceDrawerDoor : Shape
    {
        private Box _xStrip, _yStrip1, _yStrip2;
        public Side Side { get; }

        public override ViewFrom ViewFrom => ViewFrom.Outside;

        public FireplaceDrawerDoor(Side side)
        {
            Side = side;
            MainTexture = new TextureInfo(TextureKey.Wood);

            AddChild(new FireplaceDrawerDoorWindow(0, 0));
            AddChild(new FireplaceDrawerDoorWindow(1, 0));
            AddChild(new FireplaceDrawerDoorWindow(0, 1));
            AddChild(new FireplaceDrawerDoorWindow(1, 1));
            AddChild(new FireplaceDrawerDoorWindow(0, 2));
            AddChild(new FireplaceDrawerDoorWindow(1, 2));
            _xStrip = AddChild(new Box { MainTexture = new TextureInfo(TextureKey.Wood) });
            _yStrip1 = AddChild(new Box { MainTexture = new TextureInfo(TextureKey.Wood) });
            _yStrip2 = AddChild(new Box { MainTexture = new TextureInfo(TextureKey.Wood) });

        }



        protected override Triangle[] BuildInternal(QualityLevel quality)
        {
            Depth = LowerDoorThickness;
            Width = LowerDoorWidth;
            Height = Parent.Height - FooterHeight;

            Position = Parent.Position;
            this.Place().OnSideOuter(Side.South)
                .OnSideInner(Side.Top);

            var cuboid = BuildCuboid();
            var windowPlacement = CalcWindowPlacement();

            var windowTotalHeight = LowerDoorWindowHeight * 3 + LowerDoorWindowYSpacing * 2;
            _xStrip.Position = Position;
            _xStrip.Height = windowTotalHeight;
            _xStrip.Width = LowerDoorWindowXSpacing;
            _xStrip.Depth = LowerDoorWindowIndent;           
            _xStrip.Place().OnSideInner(Side.South);

            _yStrip1.Position = Position;
            _yStrip1.Height = LowerDoorWindowYSpacing;
            _yStrip1.Width = LowerDoorWindowWidth * 2 + LowerDoorWindowXSpacing;
            _yStrip1.Depth = LowerDoorWindowIndent;
            _yStrip1.Place().OnSideInner(Side.South);
            _yStrip1.Y = GetSide(Side.Top) - windowPlacement.Top - LowerDoorWindowHeight;

            _yStrip2.Position = Position;
            _yStrip2.Height = LowerDoorWindowYSpacing;
            _yStrip2.Width = LowerDoorWindowWidth * 2 + LowerDoorWindowXSpacing;
            _yStrip2.Depth = LowerDoorWindowIndent;
            _yStrip2.Place().OnSideInner(Side.South);
            _yStrip2.Y = GetSide(Side.Top) - windowPlacement.Top - (LowerDoorWindowHeight + LowerDoorWindowYSpacing) * 2;

            return new RemoveSurfaceRegion().Execute(cuboid, Side.South, windowPlacement);
        }

        private Placement2D CalcWindowPlacement()
        {
            var windowTotalWidth = LowerDoorWindowWidth * 2 + LowerDoorWindowXSpacing;
            var windowTotalHeight = LowerDoorWindowHeight * 3 + LowerDoorWindowYSpacing * 2;

            var side = (Width - windowTotalWidth) / 2.0f;
            var top = (Height - windowTotalHeight) / 2.0f;

            return new Placement2D(side, top, side, top);
        }
    }

    public class FireplaceDrawerDoorWindow : Shape
    {
        private int _x, _y;

        public FireplaceDrawerDoorWindow(int x, int y)
        {
            _x = x;
            _y = y;

            MainTexture = new TextureInfo(Color.White);
            SideTextures[Side.North] = new TextureInfo(Color.White);
            Depth = LowerDoorWindowIndent;
        }

        public override ViewFrom ViewFrom => ViewFrom.Inside;

        protected override Triangle[] BuildInternal(QualityLevel quality)
        {
            Position = Parent.Position;
            this.Place().OnSideInner(Side.South);
            var placement = CalcPlacement();

            SetSideUnanchored(Side.West, Parent.GetSide(Side.West) + placement.Left);
            SetSideUnanchored(Side.East, Parent.GetSide(Side.West) + placement.Right);
            SetSide(Side.Top, Parent.GetSide(Side.Top) - placement.Top);
            SetSideUnanchored(Side.Bottom, GetSide(Side.Top) - LowerDoorWindowHeight);

            return BuildCuboid();
        }

        private Placement2D CalcPlacement()
        {
            var windowTotalWidth = LowerDoorWindowWidth * 2 + LowerDoorWindowXSpacing;
            var windowTotalHeight = LowerDoorWindowHeight * 3 + LowerDoorWindowYSpacing * 2;

            float left, top;

            if (_x == 0)
                left = (Parent.Width - windowTotalWidth) / 2.0f;
            else
                left = Parent.Width / 2.0f + LowerDoorWindowXSpacing / 2.0f;

            top = (Parent.Height - windowTotalHeight) / 2.0f;
            top += (LowerDoorWindowHeight + LowerDoorWindowYSpacing) * _y;

            return new Placement2D(left, top, left + LowerDoorWindowWidth, top - LowerDoorWindowHeight);

        }
    }

    public class FireplaceHeatingUnitBorder : Shape
    {
        private Placement2D _heatingUnitPlacement;

        public override ViewFrom ViewFrom => ViewFrom.Outside;

        public FireplaceHeatingUnitBorder()
        {
            MainTexture = new TextureInfo(TextureKey.Wood);

            _heatingUnitPlacement = new Placement2D(HeatingUnitBorderSide, HeatingUnitBorderTop, HeatingUnitBorderSide, 0);
            AddChild(new SurfaceIndent(this, Side.South, _heatingUnitPlacement, HeatingUnitInset));
        }

        protected override Triangle[] BuildInternal(QualityLevel quality)
        {
            this.AdjustShape().From(Parent)
                              .SliceX(LowerDoorWidth, MainWidth - LowerDoorWidth * 2)
                              .SliceZ(LowerDoorThickness, MainDepth - LowerDoorThickness)
                              .SliceY(0, Parent.Height - FooterHeight);

            var cuboid = BuildCuboid();
            return new RemoveSurfaceRegion().Execute(cuboid, Side.South, _heatingUnitPlacement);
        }
    }

    public class FireplaceFooter : Shape
    {
        public override ViewFrom ViewFrom => ViewFrom.Outside;

        public FireplaceFooter()
        {
            Height = FooterHeight;
            Width = MainWidth;
            MainTexture = new TextureInfo(TextureKey.Wood);
        }

        protected override Triangle[] BuildInternal(QualityLevel quality)
        {
            Position = Parent.Position;
            this.AdjustShape().From(Parent)
                .SliceY(Parent.Height - FooterHeight, FooterHeight)
                .SliceZ(0, LowerDoorThickness);

            return BuildCuboid();

        }
    }
}
