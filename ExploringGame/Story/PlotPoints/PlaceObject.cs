using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using Microsoft.Xna.Framework;

namespace ExploringGame.Story.PlotPoints;

/// <summary>
/// Moves an object to the specific Room
/// </summary>
public class PlaceObject<TShape, TDestination> : PlotPoint
    where TDestination : IShape
    where TShape : IShape, IPhysicsShape
{
    private string _objectTag;
    private Vector3 _offset;
    private LoadedLevelData _loadedLevelData;

    public PlaceObject(string ObjectTag, Vector3 offset, LoadedLevelData loadedLevelData, params PlotPoint[] requiredDone) : base(requiredDone)
    {
        _objectTag = ObjectTag;
        _offset = offset;
        _loadedLevelData = loadedLevelData;
    }

    protected override bool CheckActivation(GameTime gameTime) => true;

    protected override void OnActivated()
    {
        var shape = _loadedLevelData.ActiveSegments.FindShape<TShape>(_objectTag);
        var room = _loadedLevelData.ActiveSegments.FindShape<TDestination>();

        shape.WorldPosition = room.WorldPosition + _offset;
        shape.InitializePhysicsObject();
        shape.Active = true;
    }

    protected override PlotUpdate UpdateActive(GameTime gameTime)
    {
        return PlotUpdate.End;
    }
}
