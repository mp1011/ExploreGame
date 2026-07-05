using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.Story.PlotPoints;

public class Delay : PlotPoint
{
    private readonly TimeSpan _delay;
    private TimeSpan _endTime;

    public Delay(TimeSpan delay, params PlotPoint[] requiredDone) : base(requiredDone) 
    {
        _delay = delay;
    }

    protected override bool CheckActivation(GameTime gameTime)
    {
        _endTime = gameTime.TotalGameTime + _delay;
        return true;
    }

    protected override PlotUpdate UpdateActive(GameTime gameTime) 
    {
        if (gameTime.TotalGameTime < _endTime)
            return PlotUpdate.Continue;
        else
            return PlotUpdate.End;        
    }
}
