using ExploringGame.GeometryBuilder.Shapes.Furniture;
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

        var openAngle = new Angle(doorSide);
        var closedAngle = doorSide == Side.East ? openAngle.RotateCounterClockwise(90) : openAngle.RotateClockwise(90);

        throw new Exception("fix me");
       // _door = AddChild(new Door(this, closedDegrees: closedAngle, openDegrees: openAngle));
      //  _door.Open = true;
    }

    protected override void BeforeBuild()
    {
        if (_doorSide == Side.East)
            _door.Hinge = new Vector3(GetSide(Side.East), this.Y, GetSide(Side.South) - _doorPlacement.Left);
        else
            _door.Hinge = new Vector3(GetSide(Side.West), this.Y, GetSide(Side.South) - _doorPlacement.Right);
    }

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        var cuboid = BuildCuboid();
        return new RemoveSurfaceRegion().Execute(cuboid, _doorSide, _doorPlacement);
    }
}
