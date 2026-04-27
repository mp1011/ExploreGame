using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Story;

public abstract class Act
{
    public int Num { get;  }

    public PlotPoint[] PlotPoints { get; }
    public Act(int num, ServiceContainer serviceContainer)
    {
        Num = num;
        PlotPoints = CreatePlotPoints(serviceContainer).ToArray();
    }

    protected abstract IEnumerable<PlotPoint> CreatePlotPoints(ServiceContainer serviceContainer);

}
