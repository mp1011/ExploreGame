using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.LevelControl;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
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
            _door = AddChild(new Door(this, Side.West, HAlign.Left, DoorDirection.Pull, StateKey.OfficeDoor1Open));
        }
        else if (doorSide == Side.West)
        {
            var closedAngle = new Angle(Side.North);
            var openAngle = new Angle(Side.West);
            _door = AddChild(new Door(this, Side.East, HAlign.Right, DoorDirection.Pull, StateKey.OfficeDoor2Open));
        }
        else
            throw new ArgumentException();
    }

    protected override void BeforeBuild()
    {
        _door.Position = Position;
        _door.SetHingePosition(_doorSide switch
        {
            Side.East => new Vector3(GetSide(Side.East), Position.Y, GetSide(Side.South) - _doorPlacement.Left),
            Side.West => new Vector3(GetSide(Side.West), Position.Y, GetSide(Side.South) - _doorPlacement.Right),
            _ => throw new ArgumentException()
        });

      //  _door.SetHingePosition(Position);
    }

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        var cuboid = BuildCuboid();
        return new RemoveSurfaceRegion().Execute(cuboid, _doorSide, _doorPlacement, ViewFrom);
    }
}
