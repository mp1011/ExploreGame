using ExploringGame.Services;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.GeometryBuilder.Shapes;

/// <summary>
/// FireplaceTop
/// FireplaceBottom
///     FirePlaceMiddleShelf
///     FirePlaceLower
/// </summary>
class ElectricFireplace : Shape
{
    public const float TopShelfHeight = 0.1f;
    public const float TopShelfOverhang = 0.05f;
    public const float MiddleShelfHeight = 0.3f;
    
    public const float MiddleShelfSide = 0.1f;
    public const float MiddleShelfTop = 0.1f;
    public const float MiddleShelfBottom = 0.1f;

    public const float MainWidth = 2.2f;
    public const float MainHeight = 1.1f;
    public const float MainDepth = 0.8f;

    public const float LowerDrawerWidth = 0.6f;


    public override ViewFrom ViewFrom =>  ViewFrom.Outside;

    public ElectricFireplace(Shape parent)
    {
        parent.AddChild(this);
        Width = MainWidth;
        Depth = MainDepth;
        Height = MainHeight;
        MainColor = Color.SandyBrown;
        SideColors[Side.Top] = Color.Brown;

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
            MainColor = Color.Brown;
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
            MainColor = Color.SandyBrown;
          
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
            MainColor = Color.HotPink;

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
    /// side drawers and heating unit
    /// </summary>
    public class FireplaceLower : Shape
    {
        public override ViewFrom ViewFrom => ViewFrom.Outside;

        public FireplaceLower(FireplaceBottom firePlace)
        {
            MainColor = Color.Brown;

            AddChild(new FireplaceLowerDrawer(Side.West));
            AddChild(new FireplaceLowerDrawer(Side.East));
            AddChild(new FireplaceHeatingUnit());

        }

        protected override Triangle[] BuildInternal(QualityLevel quality)
        {
            this.AdjustShape().From(Parent)
                         .SliceY(fromTop: MiddleShelfHeight, height: Parent.Height - MiddleShelfHeight);
            return Array.Empty<Triangle>();
        }
    }

    public class FireplaceLowerDrawer : Shape
    {
        public Side Side { get; }

        public FireplaceLowerDrawer(Side side)
        {
            Side = side;
            MainColor = Color.Orange;
        }

        public override ViewFrom ViewFrom => ViewFrom.Outside;

        protected override Triangle[] BuildInternal(QualityLevel quality)
        {
            var fromWest = 0f;
            if (Side == Side.East)
                fromWest = MainWidth - LowerDrawerWidth;
            
            this.AdjustShape().From(Parent)
                              .SliceX(fromWest, LowerDrawerWidth);

            return BuildCuboid();
        }
    }

    public class FireplaceHeatingUnit : Shape
    {
        public override ViewFrom ViewFrom => ViewFrom.Outside;

        public FireplaceHeatingUnit()
        {
            MainColor = Color.DarkGray;
        }

        protected override Triangle[] BuildInternal(QualityLevel quality)
        {
            this.AdjustShape().From(Parent)
                              .SliceX(LowerDrawerWidth, MainWidth - (LowerDrawerWidth * 2));
            return BuildCuboid();
        }
    }

}
