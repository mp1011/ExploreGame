using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.GeometryBuilder.Shapes.Furniture;

internal class Couch : PlaceableShape
{
    private static readonly float ArmWidth = Measure.Inches(8);
    private static readonly float ArmHeight = Measure.Inches(23);
    private static readonly float BackDepth = Measure.Inches(3);
    private static readonly float SeatHeight = Measure.Inches(18);

    public override ViewFrom ViewFrom =>  ViewFrom.Outside;

    public override IColliderMaker ColliderMaker => ColliderMakers.BoundingBox(this);
    public override CollisionGroup CollisionGroup => CollisionGroup.Environment;
    public override CollisionGroup CollidesWithGroups => CollisionGroup.Player | CollisionGroup.SolidEntity;
    public Couch(Shape parent)
    {
        Position = parent.Position;
        parent.AddChild(this);
        Width = Measure.Inches(72);
        Height = Measure.Inches(38);
        Depth = Measure.Inches(38);
        MainTexture = new TextureInfo(Color: Color.DarkGray, Key: TextureKey.Ceiling);


        var builder = new ShapeBuilder();
        var leftArm = builder.AddChild(this, adj => adj.SliceFromWest(0, ArmWidth)
                                                       .SliceFromTop(Height - ArmHeight, ArmHeight));
        var rightArm = builder.AddChild(this, adj => adj.SliceFromWest(Width - ArmWidth, ArmWidth)
                                                        .SliceFromTop(Height - ArmHeight, ArmHeight));
        var back = builder.AddChild(this, adj => adj.SliceFromSouth(0, BackDepth)
                                                    .SliceFromWest(ArmWidth, Width - (ArmWidth*2)));

        var seat = builder.AddChild(this, adj => adj.SliceFromBottom(0, SeatHeight)
                                                    .SliceFromWest(ArmWidth, Width - (ArmWidth * 2))
                                                    .SliceFromSouth(BackDepth, Depth - BackDepth));
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return Array.Empty<Triangle>();
    }
}
