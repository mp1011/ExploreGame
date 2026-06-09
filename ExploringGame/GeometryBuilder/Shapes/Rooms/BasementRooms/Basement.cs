using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Services;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms
{
    public class Basement : Room
    {
        private BasementOffice _office;
        private UpstairsHall _upstairsHall;

        public static readonly float InnerWallWidth = Measure.Inches(3);
        public override Theme Theme => new BasementRoomTheme();

        public BasementStairs Stairs { get; private set; }

        public DoorJunction BasementStairsDoor { get; private set; }

        public Basement(WorldSegment worldSegment, BasementOffice office) : base(worldSegment)
        {
            _office = office;

            Width = Measure.Feet(25);
            Height = Measure.Feet(8);
            Depth = Measure.Feet(28);
            SetLocalSide(Side.Bottom, 0f);

            // Create BasementStairsDoor in constructor so it's available as a dependency
            BasementStairsDoor = new DoorJunction(this, Side.South, HAlign.Right, DoorDirection.Push, StateKey.BasementStairsDoorOpen)
                { Depth = 0.5f };
            BasementStairsDoor.Tag = "BasementStairsDoor";
        }

        public void SetDependencies(UpstairsHall upstairsHall)
        {
            _upstairsHall = upstairsHall;
        }

        public void LoadChildren()
        {
            SetLocalSide(Side.East, _office.Exit.GetLocalSide(Side.West));
            SetLocalSide(Side.North, _office.Exit.GetLocalSide(Side.North) - Measure.Inches(31));

            var lightSwitch = new LightSwitch(this, Side.East, StateKey.OfficeLightOn);
            lightSwitch.Place().OnSideInner(Side.East);
            lightSwitch.SetLocalSide(Side.North, GetLocalSide(Side.North) + Measure.Inches(22));
            lightSwitch.ControlledObjects.AddRange(_office.Lights);
            lightSwitch.Place().AtStandardSwitchHeight();

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
            ceilingBar.SetLocalSide(Side.West, wall5.GetLocalSide(Side.West));
         
            // stair sides
            var wall6 = AddChild(new Box(TextureKey.Wall) { Depth = Measure.Feet(8), Height = Height, Width = InnerWallWidth * 2, OmitSides = Side.West });
            wall6.Place().OnFloor().OnSideInner(Side.South);
            wall6.SetLocalSide(Side.West, ceilingBar.GetLocalSide(Side.East) + Measure.Feet(3));
            var wall7 = AddChild(new Box(TextureKey.Wall) { Depth = Measure.Feet(8), Height = Height, Width = ceilingBar.Width,
                OmitSides = Side.East });
            wall7.Place().OnFloor().OnSideInner(Side.South).OnSideInner(Side.West, ceilingBar);

            ceilingBar.SetLocalSideUnanchored(Side.South, wall7.GetLocalSide(Side.North));
            ceilingBar.SetLocalSideUnanchored(Side.North, wall5.GetLocalSide(Side.South));

            // corner wall
            var wall8 = AddChild(new Box(TextureKey.Wall) { Depth = InnerWallWidth, Height = Height, Width = Measure.Inches(35) });
            wall8.Place().OnFloor().OnSideInner(Side.West).FromNorth(Measure.Inches(36));

            // Position BasementStairsDoor (already created in constructor)
            BasementStairsDoor.SetLocalSide(Side.Bottom, UpstairsWorldSegment.FloorY);
           
            BasementStairsDoor.SetLocalSide(Side.North, GetLocalSide(Side.South) + Measure.Feet(1.5f));

            Stairs = AddChild(new BasementStairs(WorldSegment, bottomFloor: this, topFloor: BasementStairsDoor));
            Stairs.Place().OnFloor().OnSideInner(Side.South, this, Measure.Feet(1.5f)).OnSideOuter(Side.West, wall6);
            
            BasementStairsDoor.SetLocalSide(Side.East, Stairs.GetLocalSide(Side.East) - 0.1f);
            BasementStairsDoor.SetLocalSide(Side.North, Stairs.GetLocalSide(Side.South));


            var pillar = AddChild(new Box(TextureKey.Ceiling) { Width = Measure.Inches(7), Depth = Measure.Inches(7), Height = Height - ceilingBar.Height });
            pillar.Place().At(ceilingBar).OnFloor().OnSideInner(Side.East, ceilingBar);


            // unsure why we need these
            var basementStairsDoorLeft = AddChild(new Box(TextureKey.Wall));
            var basementStairsDoorRight = AddChild(new Box(TextureKey.Wall));
            basementStairsDoorLeft.OmitSides = Side.West | Side.South | Side.East;
            basementStairsDoorRight.OmitSides = Side.West | Side.South | Side.East;

            basementStairsDoorLeft.LocalPosition = BasementStairsDoor.LocalPosition;
            basementStairsDoorLeft.Size = BasementStairsDoor.Size;
            basementStairsDoorLeft.SetLocalSide(Side.Bottom, BasementStairsDoor.GetLocalSide(Side.Bottom));
            basementStairsDoorLeft.SetLocalSideUnanchored(Side.Top, BasementStairsDoor.GetLocalSide(Side.Top));
            basementStairsDoorLeft.SetLocalSide(Side.East, Stairs.GetLocalSide(Side.East));
            basementStairsDoorLeft.SetLocalSideUnanchored(Side.West, BasementStairsDoor.GetLocalSide(Side.East));
            basementStairsDoorLeft.SetLocalSideUnanchored(Side.South, BasementStairsDoor.GetLocalSide(Side.North) + 0.5f);

            basementStairsDoorRight.LocalPosition = BasementStairsDoor.LocalPosition;
            basementStairsDoorRight.Size = BasementStairsDoor.Size;
            basementStairsDoorRight.SetLocalSide(Side.Bottom, BasementStairsDoor.GetLocalSide(Side.Bottom));
            basementStairsDoorRight.SetLocalSideUnanchored(Side.Top, BasementStairsDoor.GetLocalSide(Side.Top));
            basementStairsDoorRight.SetLocalSide(Side.West, Stairs.GetLocalSide(Side.West));
            basementStairsDoorRight.SetLocalSideUnanchored(Side.East, BasementStairsDoor.GetLocalSide(Side.West));
            basementStairsDoorRight.SetLocalSideUnanchored(Side.South, BasementStairsDoor.GetLocalSide(Side.North) + 0.5f);


            AddConnectingRoom(new RoomConnection(this, _office.Exit, Side.East, 0.5f), adjustPlacement: false);

            // Add room graph connections for stairs (without affecting positioning)
            AddConnectingRoom(new RoomConnection(this, Stairs, Side.South, 0.5f), adjustPlacement: false);

            BasementStairsDoor.AddConnectingRoom(new RoomConnection(BasementStairsDoor, Stairs, Side.North), adjustPlacement: false);

            var light = new HighHatLight(this, 3.0f, 0f);
            var lightSwitch2 = new LightSwitch(this, Side.West, StateKey.BasementLightOn);
            lightSwitch2.ControlledObjects.Add(light);

            lightSwitch2.LocalPosition = wall2.LocalPosition;
            lightSwitch2.Place().OnSideOuter(Side.West, wall2);
            lightSwitch2.Place().AtStandardSwitchHeight();

            BasementStairsDoor.AddConnectingRoom(_upstairsHall, Side.South);

            //hack
            wall6.SetLocalSideUnanchored(Side.North, Stairs.GetLocalSide(Side.North));
            wall7.SetLocalSideUnanchored(Side.North, Stairs.GetLocalSide(Side.North));

        }
    }
}
