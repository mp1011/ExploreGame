using ExploringGame.Camera;
using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using ExploringGame.LevelControl;
using ExploringGame.Services;
using Microsoft.Xna.Framework;

namespace ExploringGame.Story.PlotPoints;

public class CameraLookAt<TShape> : PlotPoint
    where TShape : Shape
{
    private readonly LoadedLevelData _loadedLevelData;
    private readonly CameraService _cameraService;
    private TShape _shape;
    private string _shapeTag;
      
    public CameraLookAt(string shapeTag, LoadedLevelData loadedLevelData, CameraService cameraService,
        params PlotPoint[] requiredDone) : base(requiredDone)
    {
        _loadedLevelData = loadedLevelData;
        _shapeTag = shapeTag;
        _cameraService = cameraService;
    }

    protected override void OnReady()
    {
        _shape = _loadedLevelData.ActiveSegments.FindShape<TShape>(_shapeTag);
    }

    protected override bool CheckActivation(GameTime gameTime) => true;

    protected override PlotUpdate UpdateActive(GameTime gameTime)
    {
        _cameraService.SetCamera(new LookAtCamera(_cameraService.Current, _shape));
        return PlotUpdate.End;
    }
}
