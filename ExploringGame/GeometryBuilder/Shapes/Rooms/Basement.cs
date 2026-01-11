using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework.Input;
using Ninject.Selection.Heuristics;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms
{
    internal class Basement : Room
    {
        public static readonly float InnerWallWidth = Measure.Inches(3);
        public override Theme Theme => new BasementRoomTheme();

        public Basement(WorldSegment worldSegment, BasementOffice office) : base(worldSegment)
        {
            Width = Measure.Feet(20);
            Height = Measure.Feet(8);
            Depth = Measure.Feet(34);
            SetSide(Side.Bottom, 0f);

            SetSide(Side.East, office.Exit.GetSide(Side.West));
            SetSide(Side.North, office.Exit.GetSide(Side.North) - Measure.Inches(31));

            var lightSwitch = new LightSwitch(this);
            lightSwitch.Place().OnSideInner(Side.East);
            lightSwitch.SetSide(Side.North, GetSide(Side.North) + Measure.Inches(22));
            lightSwitch.ControlledObjects.AddRange(office.Lights);
            lightSwitch.Y -= 0.5f;

            // L-shaped walls
            var wall1 = AddChild(new Box(TextureKey.Wall) { Depth = InnerWallWidth, Height = Height, Width = Measure.Inches(31) });
            wall1.Place().OnFloor().OnSideInner(Side.East).FromNorth(Measure.Inches(31 + 36 + 31));
            var wall2 = AddChild(new Box(TextureKey.Wall) { Depth = Measure.Inches(35), Height = Height, Width = InnerWallWidth });
            wall2.Place().OnFloor().OnSideInner(Side.West, wall1).OnSideOuter(Side.South, wall1);

            // boiler cover
            var wall3 = AddChild(new Box(TextureKey.Wall) { Depth = Measure.Inches(58), Height = Height, Width = InnerWallWidth });
            wall3.Place().OnFloor().OnSideInner(Side.North).FromEast(Measure.Inches(53));
            var wall4 = AddChild(new Box(TextureKey.Wall) { Depth = InnerWallWidth, Height = Height, Width = Measure.Inches(69) });
            wall4.Place().OnFloor().OnSideInner(Side.South, wall3).OnSideOuter(Side.West, wall3);
            var wall5 = AddChild(new Box(TextureKey.Wall) { Depth = Measure.Inches(58), Height = Height, Width = InnerWallWidth });
            wall5.Place().OnFloor().OnSideInner(Side.South, wall4).OnSideOuter(Side.West, wall4);

            var ceilingBar = AddChild(new Box(TextureKey.Ceiling) { Width = Measure.Inches(12), Height = Measure.Inches(9), Depth = Depth });
            ceilingBar.Place().OnSideInner(Side.Top);
            ceilingBar.SetSide(Side.West, wall5.GetSide(Side.West));

            var pillar = AddChild(new Box(TextureKey.Ceiling) { Width = Measure.Inches(7), Depth = Measure.Inches(7), Height = Height - ceilingBar.Height });
            pillar.Place().OnFloor().OnSideInner(Side.East, ceilingBar);

            // stair sides
            var wall6 = AddChild(new Box(TextureKey.Wall) { Depth = Measure.Feet(8), Height = Height, Width = InnerWallWidth * 2 });
            wall6.Place().OnFloor().OnSideInner(Side.South);
            wall6.SetSide(Side.West, ceilingBar.GetSide(Side.East) + Measure.Feet(3));
            var wall7 = AddChild(new Box(TextureKey.Wall) { Depth = Measure.Feet(8) + Measure.Inches(5), Height = Height, Width = ceilingBar.Width });
            wall7.Place().OnFloor().OnSideInner(Side.South).OnSideInner(Side.West, ceilingBar);

            ceilingBar.SetSideUnanchored(Side.South, wall7.GetSide(Side.North));
            ceilingBar.SetSideUnanchored(Side.North, wall5.GetSide(Side.South));

            // corner wall
            var wall8 = AddChild(new Box(TextureKey.Wall) { Depth = InnerWallWidth, Height = Height, Width = Measure.Inches(35) });
            wall8.Place().OnFloor().OnSideInner(Side.West).FromNorth(Measure.Inches(36));

            var stairs = AddChild(new BasementStairs(worldSegment, this));
            stairs.Place().OnFloor().OnSideInner(Side.South).OnSideOuter(Side.West, wall6);


        }
    }
}
