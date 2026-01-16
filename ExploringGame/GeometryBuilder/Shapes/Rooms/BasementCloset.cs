using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.LevelControl;
using ExploringGame.Services;
using ExploringGame.Texture;
using System;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms;

public class BasementCloset : Shape
{
    private Side _doorSide;
    private Placement2D _doorPlacement = new Placement2D(Left: Measure.Inches(3), Right: Measure.Inches(7),
                Bottom: 0, Top: 0);
    private Door _door;

    public BasementCloset(Shape parent, Side doorSide)
    {
        _doorSide = doorSide;
        parent.AddChild(this);
        Height = parent.Height;
        Width = Measure.Inches(36);
        Depth = Measure.Inches(39);
        MainTexture = new TextureInfo(TextureKey.Wall);

        var interior = AddChild(new SurfaceIndent(this, doorSide, _doorPlacement, Measure.Inches(35),
            displayFaces: (Side.North | Side.South | Side.East | Side.West) & ~doorSide));
        interior.MainTexture = new TextureInfo(TextureKey.Ceiling);

        if (doorSide == Side.East)
        {
            var closedAngle = new Angle(Side.North);
            var openAngle = new Angle(Side.East);
            _door = AddChild(new Door(this, closedDegrees: closedAngle, openDegrees: openAngle, hingeSide: HAlign.Left, stateKey: StateKey.OfficeDoor1Open));
        }
        else if (doorSide == Side.West)
        {
            var closedAngle = new Angle(Side.North);
            var openAngle = new Angle(Side.West);
            _door = AddChild(new Door(this, closedDegrees: closedAngle, openDegrees: openAngle, hingeSide: HAlign.Right, stateKey: StateKey.OfficeDoor2Open));
        }
        else
            throw new ArgumentException();
    }

    protected override void BeforeBuild()
    {
        _door.Position = Position;

        // todo, door placement is tricky         
        if (_doorSide == Side.East)
        {
            _door.X = GetSide(Side.East) + _door.Width / 2f;
            _door.Z += (_door.Width / 2f) + 0.1f;
        }
        else
        {
            _door.X = GetSide(Side.West) - _door.Width / 2f;
            _door.Z += (_door.Width / 2f) - 0.1f;
        }
    }

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        var cuboid = BuildCuboid();
        return new RemoveSurfaceRegion().Execute(cuboid, _doorSide, _doorPlacement, ViewFrom);
    }
}
