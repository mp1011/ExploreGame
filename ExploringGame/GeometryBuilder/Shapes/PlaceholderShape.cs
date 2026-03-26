using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes;


public abstract class PlaceholderShape : Room
{
    public override ViewFrom ViewFrom => ViewFrom.None;

    public PlaceholderShape(WorldSegment worldSegment) : base(worldSegment)
    {

    }

    public abstract Room FindMatchingRealShape(IEnumerable<LevelData> levelData);
}

// non-rendered shape that acts as a placeholder for a shape in another WorldSegment
public class PlaceholderShape<T> : PlaceholderShape
    where T : Room
{
    public PlaceholderShape(WorldSegment worldSegment, string tag, Vector3 position, Vector3 size) : base(worldSegment)
    {
        Position = position;
        Size = size;
        Tag = tag;
    }

    public PlaceholderShape(WorldSegment worldSegment, Vector3 position, Vector3 size) : base(worldSegment)
    {
        Position = position;
        Size = size;
    }

    public override Room FindMatchingRealShape(IEnumerable<LevelData> levelData)
    {
        var typeMatches = levelData.SelectMany(p => p.WorldSegment.TraverseAllChildren().OfType<T>()).ToArray();

        if (Tag != null)
            return typeMatches.SingleOrDefault(p => p.Tag == Tag) as Room;
        else
            return typeMatches.SingleOrDefault() as Room;
    }

    public override string ToString()
    {
        return $"Placeholder: {typeof(T).Name} {Tag}".Trim();
    }
}
