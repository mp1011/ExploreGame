using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Services;

public class BlockerCreator
{
    public IEnumerable<Blocker> Execute(WorldSegment worldSegment, IEnumerable<Shape> shapes)
    {
        return shapes.Select(s => worldSegment.AddChild(new Blocker(s))).ToArray();
    }

    public IEnumerable<Blocker> ExecuteForDoors(WorldSegment worldSegment, params StateKey[] doorKeys)
    {
        var shapes = worldSegment.TraverseAllChildren().OfType<Door>()
            .Where(p => doorKeys.Contains(p.StateKey))
            .Select(p => p.Parent)
            .Distinct();

        return Execute(worldSegment, shapes);
    }

    public IEnumerable<Blocker> ExecuteForSwitches(WorldSegment worldSegment, params StateKey[] switchKeys)
    {
        var shapes = worldSegment.TraverseAllChildren().OfType<LightSwitch>()
            .Where(p => switchKeys.Contains(p.StateKey));

        return Execute(worldSegment, shapes);
    }
}
