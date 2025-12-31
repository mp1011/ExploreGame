using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using System;

namespace ExploringGame.Services;

public class ShapeBuilder
{
    public ComplexShapePart AddChild(Shape parent, Action<ShapeAdjuster> adjust)
    {
        var part = new ComplexShapePart(ViewFrom.Outside, adjust);
        return AddChild(parent, part);
    }

    public ComplexShapePart AddChild(Shape parent, ComplexShapePart part)
    {
        part.MainTexture = parent.MainTexture;
        foreach (var sideTexture in parent.SideTextures)
            part.SideTextures[sideTexture.Key] = sideTexture.Value;

        parent.AddChild(part);
        return part;
    }
}
