using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Story;

public enum PlotPointState
{
    Idle,
    Ready,
    Active,
    Done
}

public enum PlotUpdate
{
    Continue,
    End,
}

public static class PlotPointExtensions
{
    public static IEnumerable<PlotPoint> ByState(this IEnumerable<PlotPoint> points, PlotPointState state) =>
        points.Where(p => p.State == state);
}

public abstract class PlotPoint
{
    public PlotPointState State { get; private set; }

    public PlotPoint[] RequiredDone { get; }


    public PlotPoint(IEnumerable<PlotPoint> requiredDone)
    {
        RequiredDone = requiredDone.ToArray();
    }

    public void Update(GameTime gameTime)
    {
        State = UpdateState(gameTime);
    }

    private PlotPointState UpdateState(GameTime gameTime)
    {
        switch (State)
        {
            case PlotPointState.Idle:
                if (RequiredDone.Any(p => p.State != PlotPointState.Done))
                    return PlotPointState.Idle;
                else
                    return PlotPointState.Ready;
            case PlotPointState.Ready:
                //todo
                return PlotPointState.Active;
            case PlotPointState.Active:
                var updateResult = UpdateActive(gameTime);
                if (updateResult == PlotUpdate.End)
                    return PlotPointState.Done;
                else
                    return PlotPointState.Active;
            default:
                return State;
        }
    }


    public abstract PlotUpdate UpdateActive(GameTime gameTime);
}
