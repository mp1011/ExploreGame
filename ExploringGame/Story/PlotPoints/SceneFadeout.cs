using ExploringGame.Services;
using Microsoft.Xna.Framework;

namespace ExploringGame.Story.PlotPoints;

public class SceneFadeout : PlotPoint
{
    private float _fadeStep = 0.01f;

    private RenderTargetTransformService _renderTargetTransformService;

    public SceneFadeout(RenderTargetTransformService renderTargetTransformService, params PlotPoint[] requiredDone)
        :base(requiredDone)
    {
        _renderTargetTransformService = renderTargetTransformService;
    }

    protected override bool CheckActivation(GameTime gameTime) => true;

    protected override PlotUpdate UpdateActive(GameTime gameTime)
    {
        if (_renderTargetTransformService.Brightness > _fadeStep)
        {
            _renderTargetTransformService.Brightness -= _fadeStep;
            return PlotUpdate.Continue;
        }

        _renderTargetTransformService.Brightness = 0;
        return PlotUpdate.NextScene;
    }

    protected override PlotUpdate FastForward_Inner()
    {
        _renderTargetTransformService.Brightness = 0;
        return PlotUpdate.NextScene;
    }
}
