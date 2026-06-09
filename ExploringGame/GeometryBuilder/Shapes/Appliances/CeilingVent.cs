using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Logics.ShapeControllers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.GeometryBuilder.Shapes.Appliances;

public class CeilingVent : Shape, ICutoutShape
{
    public override Theme Theme => new Theme(Color.DarkGray);
    public override ViewFrom ViewFrom => ViewFrom.Inside;

    public Side ParentCutoutSide => Side.Top;

    Triangle[] ICutoutShape.Build() => BuildInternal(QualityLevel.Basic);

    public CeilingVent(Room room, float x, float z)
    {
        LocalX = room.LocalX + x;
        LocalY = room.LocalY;
        LocalZ = room.LocalZ + z;
        room.AddChild(this);

        Height = Measure.Feet(2);
        Width = Measure.Inches(6); 
        Depth = Measure.Inches(12);

        this.Place().OnSideOuter(Side.Top, room);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }

    public LightController<HighHatLight> Controller { get; private set; }    
}
