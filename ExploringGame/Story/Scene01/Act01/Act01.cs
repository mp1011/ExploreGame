using ExploringGame.Story.PlotPoints;
using System.Collections.Generic;

namespace ExploringGame.Story.Scene01.Act01;

public class ActOne : Act
{
    public ActOne(ServiceContainer serviceContainer) : base(1, serviceContainer)
    {
    }

    protected override IEnumerable<PlotPoint> CreatePlotPoints(ServiceContainer serviceContainer)
    {
        yield return serviceContainer.Get<DebugMessage>();
    }
}
