using ExploringGame.GeometryBuilder;
using ExploringGame.LevelControl;
using Microsoft.Xna.Framework;

namespace ExploringGame.Story.PlotPoints;

public class CameraLookAt<TShape> : PlotPoint
    where TShape : Shape
{
    private TShape _shape;
    private string _shapeTag;
    protected readonly LoadedLevelData _loadedLevelData;
   
    public CameraLookAt(string shapeTag, LoadedLevelData loadedLevelData, params PlotPoint[] requiredDone) : base(requiredDone)
    {
        _loadedLevelData = loadedLevelData;
        _shapeTag = shapeTag;
    }

    protected override void OnReady()
    {
        _shape = _loadedLevelData.ActiveSegments.FindShape<TShape>(_shapeTag);
    }

    protected override bool CheckActivation(GameTime gameTime) => true;

    protected override PlotUpdate UpdateActive(GameTime gameTime)
    {
        return PlotUpdate.End;
    }
}
