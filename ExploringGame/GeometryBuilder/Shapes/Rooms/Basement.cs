using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Services;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms
{
    internal class Basement : Room
    {
        private Room _upstairs;
        private BasementOffice _office;

        public static readonly float InnerWallWidth = Measure.Inches(3);
        public override Theme Theme => new BasementRoomTheme();

        public BasementStairs Stairs { get; private set; }

        public Basement(WorldSegment worldSegment, BasementOffice office, Room upstairs) : base(worldSegment)
        {
            _upstairs = upstairs;
            _office = office;

            Width = Measure.Feet(20);
            Height = Measure.Feet(8);
            Depth = Measure.Feet(34);
            SetSide(Side.Bottom, 0f);
        }

        public override void LoadChildren()
        {           
            SetSide(Side.East, _office.Exit.GetSide(Side.West));
            SetSide(Side.North, _office.Exit.GetSide(Side.North) - Measure.Inches(31));

            var lightSwitch = new LightSwitch(this, StateKey.OfficeLightOn);
            lightSwitch.Place().OnSideInner(Side.East);
            lightSwitch.SetSide(Side.North, GetSide(Side.North) + Measure.Inches(22));
            lightSwitch.ControlledObjects.AddRange(_office.Lights);
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
            var wall6 = AddChild(new Box(TextureKey.Wall) { Depth = Measure.Feet(8), Height = Height, Width = InnerWallWidth * 2, OmitSides = Side.West });
            wall6.Place().OnFloor().OnSideInner(Side.South);
            wall6.SetSide(Side.West, ceilingBar.GetSide(Side.East) + Measure.Feet(3));
            var wall7 = AddChild(new Box(TextureKey.Wall) { Depth = Measure.Feet(8), Height = Height, Width = ceilingBar.Width,
                OmitSides = Side.East});
            wall7.Place().OnFloor().OnSideInner(Side.South).OnSideInner(Side.West, ceilingBar);

            ceilingBar.SetSideUnanchored(Side.South, wall7.GetSide(Side.North));
            ceilingBar.SetSideUnanchored(Side.North, wall5.GetSide(Side.South));

            // corner wall
            var wall8 = AddChild(new Box(TextureKey.Wall) { Depth = InnerWallWidth, Height = Height, Width = Measure.Inches(35) });
            wall8.Place().OnFloor().OnSideInner(Side.West).FromNorth(Measure.Inches(36));

            Stairs = AddChild(new BasementStairs(WorldSegment, bottomFloor: this, topFloor: _upstairs));
            Stairs.Place().OnFloor().OnSideInner(Side.South, this).OnSideOuter(Side.West, wall6);

            AddConnectingRoom(new RoomConnection(this, _office.Exit, Side.East, 0.5f), adjustPlacement: false);
 
            _upstairs.AddConnectingRoom(new RoomConnection(_upstairs, Stairs, Side.North), adjustPlacement: false);

            var light = new HighHatLight(this, 1.0f, 0f);
            var lightSwitch2 = new LightSwitch(this, StateKey.BasementLightOn);
            lightSwitch2.ControlledObjects.Add(light);

            lightSwitch2.Position = wall2.Position;
            lightSwitch2.Place().OnSideOuter(Side.West, wall2);
            lightSwitch2.Y -= 0.5f;
        }
    }
}
