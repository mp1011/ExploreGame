using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Logics.ShapeControllers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.GeometryBuilder.Shapes.Structures;

public class SlidingDoorJunction : Room
{
    private Side _wallSide;
    private SlidingDoorPane _fixedPane;
    private MovingSlidingDoorPane _movingPane;

    public SlidingDoorJunction(Room room, Side wallSide, HAlign closedDirection, StateKey doorStateKey) : base(room.WorldSegment)
    {
        _wallSide = wallSide;
        if (wallSide.GetAxis() == Axis.Z)
        {
            Width = Door.StandardWidth * 2.2f;
            Depth = 0.2f;
            Height = room.Height;
        }
        else
        {
            Depth = Door.StandardWidth * 2.2f;
            Width = 0.2f;
            Height = room.Height;
        }

        MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);

        _fixedPane = new SlidingDoorPane(this, wallSide);
        _movingPane = new MovingSlidingDoorPane(this, wallSide);
    }


    protected override void BeforeBuild()
    {
        _fixedPane.PlacePane(_wallSide.CounterClockwiseTurn());
        _movingPane.PlacePane(_wallSide.ClockwiseTurn());

        _fixedPane.WorldPosition = _fixedPane.WorldPosition + _wallSide.AsVector() * -0.1f;
        _movingPane.WorldPosition = _movingPane.WorldPosition + _wallSide.AsVector() * 0.1f;

    }
}

public class SlidingDoorPane : PlaceableShape, IPlaceableObject
{
    private readonly float PartSize = Measure.Inches(6);
    protected Side _wallSide;

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public override Theme Theme => new UpstairsHallTheme();

    public override IColliderMaker ColliderMaker => ColliderMakers.BoundingBox(this);
    public override CollisionGroup CollisionGroup => CollisionGroup.Doors;
    public override CollisionGroup CollidesWithGroups => CollisionGroup.MovingObjects;

    public virtual float WidthPercent => 0.6f;

    public SlidingDoorPane(SlidingDoorJunction junction, Side wallSide)
    {
        junction.AddChild(this);
        _wallSide = wallSide;
    }

    public void PlacePane(Side placementSide)
    {
        LocalPosition = Vector3.Zero;

        var sideAxis = _wallSide.ClockwiseTurn().GetAxis();
        var thicknessAxis = _wallSide.GetAxis();

        this.AdjustShape().SetAxis(sideAxis, Parent.GetAxisSize(sideAxis) * WidthPercent)
            .SetAxis(thicknessAxis, Parent.GetAxisSize(thicknessAxis) * 0.3f)
            .SetAxis(Axis.Y, Parent.Height);

        var bottom = AddChild(new Box(Theme, TextureKey.Plain));
        bottom.AdjustShape().From(this).SliceFromBottom(0, PartSize);
        bottom.Place().OnFloor(this);

        var top = AddChild(new Box(Theme, TextureKey.Plain));
        top.AdjustShape().From(this).SliceFromTop(0, PartSize);
        top.Place().OnSideInner(Side.Top);

        var side1 = AddChild(new Box(Theme, TextureKey.Plain));
        var side2 = AddChild(new Box(Theme, TextureKey.Plain));
        var pane = AddChild(new GlassPane(_wallSide, hasCollision: false));

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

        this.Place()
           .At(Parent)
           .OnFloor()
           .OnSideInner(placementSide);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return Array.Empty<Triangle>();
    }
}

public class MovingSlidingDoorPane : SlidingDoorPane, IPlaceableObject, IControllable<SlidingDoorController>
{
    public Side OpenSide => _wallSide.CounterClockwiseTurn();

    public Axis OpenAxis => OpenSide.GetAxis();

    public SlidingDoorController Controller { get; private set; }

    public override IColliderMaker ColliderMaker => new SlidingDoorColliderMaker(this);

    public override float WidthPercent => 0.5f;

    public MovingSlidingDoorPane(SlidingDoorJunction junction, Side wallSide) : base(junction, wallSide)
    {
    }

    public IActiveObject CreateController(ServiceContainer serviceContainer)
    {
        var controller = serviceContainer.Get<SlidingDoorController>();
        controller.Shape = this;
        Controller = controller;
        return controller;
    }
}
