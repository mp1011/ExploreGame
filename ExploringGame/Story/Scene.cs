using ExploringGame.GeometryBuilder.Shapes.WorldSegments;

namespace ExploringGame.Story;

public abstract class Scene
{
    public Act[] Acts { get; }
    public WorldSegmentGroup WorldSegmentGroup { get; }

    public Scene(WorldSegmentGroup worldSegmentGroup, params Act[] acts)
    {
        Acts = acts;
        WorldSegmentGroup = worldSegmentGroup;
    }

}


