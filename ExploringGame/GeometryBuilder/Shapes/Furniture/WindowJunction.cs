using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ExploringGame.GeometryBuilder.Shapes.Furniture;

/// <summary>
/// Mini "room" which joins an interior room to an exterior room, creating a window opening.
/// The junction itself represents the windowsill.
/// </summary>
public class Window : Room
{
    private readonly float SillOverhang = Measure.Inches(4);
    private readonly float SillThickness = Measure.Inches(4);
    private readonly float SillHeight = Measure.Feet(2.0f); // position above floor
    private readonly float WindowDepth = 0.2f;

    private readonly float RodThickness = Measure.Inches(2.0f);
    private readonly float RodLengthExtra = Measure.Inches(8.0f);
    private readonly float RodWallOffset = Measure.Inches(8.0f);
    private readonly float RodVerticalOffset = Measure.Inches(-2.0f);
    private readonly float RodEndcapRadius = Measure.Inches(3.0f);

    private readonly float CurtainWidth = Measure.Feet(1.0f);
    private readonly float CurtainDistanceFromFloor = Measure.Feet(1.0f);


    private Side _wallSide;
    private Room _parentRoom;
    private Room _exteriorRoom;

    public Window(Room room, Side wallSide, float width, float height, HAlign align = HAlign.Center, float offset = 0f) : base(room.WorldSegment)
    {
        room.AddChild(this);
        _parentRoom = room;
        _wallSide = wallSide;
        MainTexture = new TextureInfo(Color.White, TextureKey.Wall);

        this.AdjustShape()
            .SetAxis(wallSide.GetAxis(), WindowDepth)
            .SetAxis(wallSide.GetPerpendicularAxis(), width)
            .SetAxis(Axis.Y, height);

        _exteriorRoom = CreateDummyExterior();
        _exteriorRoom.Place().OnSideOuter(_wallSide, this);
        _parentRoom.AddConnectingRoomWithJunction(this, _exteriorRoom, _wallSide, align, offset);

        this.Place().FromSide(Side.Bottom, SillHeight);

        // window sill
        new ShapeBuilder().AddChild(this, a => a
            .SliceFromBottom(0, SillThickness)
            .AxisStretch(_wallSide.GetAxis(), SillOverhang));

        // curtain rod
        var rod = AddChild(new Cylinder { Axis = wallSide.GetPerpendicularAxis() });
        rod.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Ceiling);
        rod.AdjustShape()
            .SetAxis(Axis.Y, RodThickness)
            .SetAxis(_wallSide.GetAxis(), RodThickness)
            .SetAxis(_wallSide.GetPerpendicularAxis(), width + (RodLengthExtra * 2f));
        rod.Place()
            .AtParent()
            .OnSideInner(wallSide, this, offset: -RodWallOffset * _wallSide.Sign())
            .FromSide(Side.Top, RodVerticalOffset);

        // rod end-caps
        var cap1 = AddChild(new Ellipsoid(radius: RodEndcapRadius));
        var cap2 = AddChild(new Ellipsoid(radius: RodEndcapRadius));
        cap1.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Ceiling);
        cap2.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Ceiling);
        cap1.Place().At(rod).OnSideOuter(wallSide.ClockwiseTurn(), rod);
        cap2.Place().At(rod).OnSideOuter(wallSide.CounterClockwiseTurn(), rod);

        // curtains
        CreateCurtain(rod, wallSide, wallSide.ClockwiseTurn());
        CreateCurtain(rod, wallSide, wallSide.CounterClockwiseTurn());
    }

    private Box CreateCurtain(Shape rod, Side wallSide, Side curtainSide)
    {
        var curtain = AddChild(new Box());
        curtain.AdjustShape()
            .SetAxis(_wallSide.GetAxis(), 0.01f)
            .SetAxis(Axis.Y, 1.0f) // placeholder
            .SetAxis(_wallSide.GetPerpendicularAxis(), CurtainWidth);

        curtain.MainTexture = new TextureInfo(Color.Purple, TextureKey.Ceiling);
        curtain.OmitSides = Side.All & ~wallSide.Opposite();
        curtain.Place().At(rod)
                        .OnSideInner(curtainSide, rod)
                        .OnSideOuter(wallSide.Opposite(), rod)
                        .OnSideOuter(Side.Bottom, rod);
        curtain.SetSideUnanchored(Side.Bottom, _parentRoom.GetSide(Side.Bottom) + CurtainDistanceFromFloor);
        return curtain;
    }

    private Room CreateDummyExterior()
    {
        // Create dummy exterior room (larger than window in both axes from viewer's perspective)
        float exteriorOpeningWidth = (_wallSide.GetAxis() == Axis.Z) ? Width : Depth;
        float exteriorOpeningHeight = Height;
        float exteriorThickness = 0.4f;
        float exteriorExtra = 0.8f; // How much larger the exterior is than the window

        float exteriorRoomWidth, exteriorRoomDepth, exteriorRoomHeight;
        if (_wallSide.GetAxis() == Axis.Z)
        {
            exteriorRoomWidth = exteriorOpeningWidth + exteriorExtra;
            exteriorRoomDepth = Depth + exteriorThickness;
            exteriorRoomHeight = exteriorOpeningHeight + exteriorExtra;
        }
        else
        {
            exteriorRoomWidth = Width + exteriorThickness;
            exteriorRoomDepth = exteriorOpeningWidth + exteriorExtra;
            exteriorRoomHeight = exteriorOpeningHeight + exteriorExtra;
        }

        var exteriorRoom = new Room(_parentRoom.WorldSegment, exteriorRoomWidth, exteriorRoomDepth, exteriorRoomHeight);
        exteriorRoom.MainTexture = new TextureInfo(Color.White);
        exteriorRoom.FixedAmbientLight = LightIntensity.VeryBright;
        return exteriorRoom;
    }

    public override string ToString()
    {
        return "Window: " + string.Join(" - ", RoomConnections.Select(p => p.GetOtherRoom(this).ToString()).ToArray());
    }
}
