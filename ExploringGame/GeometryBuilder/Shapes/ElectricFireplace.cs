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

    public override ViewFrom ViewFrom =>  ViewFrom.Outside;

    public ElectricFireplace(Shape parent)
    {
        parent.AddChild(this);
        Width = 2.2f;
        Depth = 0.8f;
        Height = 1.1f;
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
        public override ViewFrom ViewFrom => ViewFrom.Outside;

        public FirePlaceMiddleShelf(FireplaceBottom firePlace)
        {
            MainColor = Color.HotPink;
        }

        protected override Triangle[] BuildInternal(QualityLevel quality)
        {
            this.AdjustShape().From(Parent)
                         .SliceY(fromTop: 0, height: MiddleShelfHeight);

            // thought, maybe shape building should be its own service?
            var cuboid = BuildCuboid();
            return new RemoveSurfaceRegion().Execute(cuboid, Side.South, new Placement2D(Left: MiddleShelfSide, Right: MiddleShelfSide,
                Bottom: MiddleShelfBottom, Top: MiddleShelfTop));
        }
    }

    public class FireplaceLower : Shape
    {
        public override ViewFrom ViewFrom => ViewFrom.Outside;

        public FireplaceLower(FireplaceBottom firePlace)
        {
            MainColor = Color.Brown;
        }

        protected override Triangle[] BuildInternal(QualityLevel quality)
        {
            this.AdjustShape().From(Parent)
                         .SliceY(fromTop: MiddleShelfHeight, height: Parent.Height - MiddleShelfHeight);
            return BuildCuboid();
        }
    }

}
