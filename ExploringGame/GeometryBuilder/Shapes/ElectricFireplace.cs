using ExploringGame.Services;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes
{
    class ElectricFireplace : Shape
    {
        public const float TopShelfHeight = 0.1f;
        public const float TopShelfOverhang = 0.05f;

        public override ViewFrom ViewFrom =>  ViewFrom.Outside;

        public ElectricFireplace(Shape parent)
        {
            parent.AddChild(this);
            Width = 2.2f;
            Depth = 0.8f;
            Height = 1.1f;
            MainColor = Color.SandyBrown;
            SideColors[Side.Top] = Color.Brown;
        }

        protected override Triangle[] BuildInternal(QualityLevel quality)
        {
            return Shape.BuildMany(quality, new FireplaceTop(this), new FireplaceBottom(this));
        }

        /// <summary>
        /// top shelf
        /// </summary>
        public class FireplaceTop : Shape
        {
            private readonly ElectricFireplace _firePlace;
            public override ViewFrom ViewFrom => ViewFrom.Outside;

            public FireplaceTop(ElectricFireplace firePlace)
            {
                _firePlace = firePlace;
                firePlace.AddChild(this);
                MainColor = Color.Brown;

                this.Adjust().From(firePlace)
                             .SliceY(fromTop: 0.0f, height: TopShelfHeight)
                             .AxisStretch(Axis.X | Axis.Z, TopShelfOverhang);
            }

            protected override Triangle[] BuildInternal(QualityLevel quality)
            {
                return BuildCuboid();
            }

        }

        /// <summary>
        /// base, drawers, fireplace, and middle shelves
        /// </summary>
        public class FireplaceBottom : Shape
        {
            private readonly ElectricFireplace _firePlace;
            public override ViewFrom ViewFrom => ViewFrom.Outside;

            public FireplaceBottom(ElectricFireplace firePlace)
            {
                _firePlace = firePlace;
                firePlace.AddChild(this);
                MainColor = Color.SandyBrown;

                this.Adjust().From(firePlace)
                             .SliceY(fromTop: TopShelfHeight, height: firePlace.Height - TopShelfHeight);
            }
            protected override Triangle[] BuildInternal(QualityLevel quality)
            {
                return BuildCuboid();
            }
        }
    }
}
