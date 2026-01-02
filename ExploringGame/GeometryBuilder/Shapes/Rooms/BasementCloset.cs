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

        var interior = AddChild(new SurfaceIndent(this, doorSide, _doorPlacement, Measure.Inches(35)));
        interior.MainTexture = new TextureInfo(TextureKey.Ceiling);

        _door = AddChild(new Door(this, 270f, 190f));
    }

    protected override void BeforeBuild()
    {
        _door.Position = Position;
        //temp
        _door.X += 1.0f;

        _door.Hinge = new Vector3(GetSide(Side.East), this.Y, GetSide(Side.South));
    }

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        var cuboid = BuildCuboid();
        return new RemoveSurfaceRegion().Execute(cuboid, _doorSide, _doorPlacement);
    }
}
