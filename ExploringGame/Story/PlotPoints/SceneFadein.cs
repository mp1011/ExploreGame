using ExploringGame.Services;
using Microsoft.Xna.Framework;

namespace ExploringGame.Story.PlotPoints;

public class SceneFadein : PlotPoint
{
    private float _fadeStep = 0.01f;

    private RenderTargetTransformService _renderTargetTransformService;

    public SceneFadein(RenderTargetTransformService renderTargetTransformService, params PlotPoint[] requiredDone)
        :base(requiredDone)
    {
        _renderTargetTransformService = renderTargetTransformService;
    }

    protected override bool CheckActivation(GameTime gameTime) => true;

    protected override PlotUpdate UpdateActive(GameTime gameTime)
    {
        if (_renderTargetTransformService.Brightness < 1f - _fadeStep)
        {
            _renderTargetTransformService.Brightness += _fadeStep;
            return PlotUpdate.Continue;
        }

        _renderTargetTransformService.Brightness = 1f;
        return PlotUpdate.End;
    }
}
