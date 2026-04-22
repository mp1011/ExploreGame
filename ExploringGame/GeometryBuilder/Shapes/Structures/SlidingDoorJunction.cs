using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.LevelControl;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ExploringGame.GeometryBuilder.Shapes.Structures;

public class SlidingDoorJunction : Room
{
    private Side _wallSide;
    private SlidingDoorPane _pane1, _pane2;

    public SlidingDoorJunction(Room room, Side wallSide, HAlign closedDirection, StateKey doorStateKey) : base(room.WorldSegment)
    {
        _wallSide = wallSide;
        if (wallSide.GetAxis() == Axis.Z)
        {
            Width = Door.StandardWidth * 2;
            Depth = 0.2f;
            Height = room.Height;
        }
        else
        {
            Depth = Door.StandardWidth * 2;
            Width = 0.2f;
            Height = room.Height;
        }

        MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);

        _pane1 = new SlidingDoorPane(this, wallSide);
      //  _pane2 = new SlidingDoorPane(this, wallSide);

    }


    protected override void BeforeBuild()
    {
        _pane1.PlacePane();
    }    
}

public class SlidingDoorPane : Shape
{
    private readonly float PartSize = Measure.Inches(6);
    private Side _wallSide;

    public override ViewFrom ViewFrom => ViewFrom.None;     

    public override Theme Theme => new UpstairsHallTheme();

    public SlidingDoorPane(SlidingDoorJunction junction, Side wallSide)
    {
        junction.AddChild(this);
        _wallSide = wallSide;
    }

    public void PlacePane()
    {
        var sideAxis = _wallSide.ClockwiseTurn().GetAxis();
        var thicknessAxis = _wallSide.GetAxis();

        this.AdjustShape().SetAxis(sideAxis, Parent.GetAxisSize(sideAxis) * 0.6f)
            .SetAxis(thicknessAxis, Parent.GetAxisSize(thicknessAxis) * 0.3f)
            .SetAxis(Axis.Y, Parent.Height);

        this.Place()
            .At(Parent)
            .OnFloor()
            .OnSideInner(_wallSide.ClockwiseTurn());

        var bottom = AddChild(new Box(Theme, TextureKey.Plain));
        bottom.AdjustShape().From(this).SliceFromBottom(0, PartSize);
        bottom.Place().OnFloor(this);

        var top = AddChild(new Box(Theme, TextureKey.Plain));
        top.AdjustShape().From(this).SliceFromTop(0, PartSize);
        top.Place().OnSideInner(Side.Top);

        var side1 = AddChild(new Box(Theme, TextureKey.Plain));
        var side2 = AddChild(new Box(Theme, TextureKey.Plain));
        var pane = AddChild(new GlassPane(_wallSide));

        pane.AdjustShape().From(this)
            .SetAxis(thicknessAxis, Measure.Inches(2))
            .SetAxis(sideAxis, GetAxisSize(sideAxis) - PartSize * 2)
            .SliceFromTop(PartSize, Height - PartSize * 2);

        if (sideAxis == Axis.X)
        {
            side1.AdjustShape().From(this)
                .SliceFromWest(0, PartSize)
                .SliceFromTop(PartSize, Height - PartSize * 2);

            side2.AdjustShape().From(this)
                .SliceFromEast(0, PartSize)
                .SliceFromTop(PartSize, Height - PartSize * 2);
        }
        else
        {
            side1.AdjustShape().From(this)
               .SliceFromNorth(0, PartSize);
            side2.AdjustShape().From(this)
                .SliceFromSouth(0, PartSize);
        }

    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }
}
