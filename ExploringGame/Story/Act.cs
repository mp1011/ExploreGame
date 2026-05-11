using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Story;

public abstract class Act
{
    public int Num { get;  }

    public PlotPoint[] PlotPoints { get; }
    public Act(int num, PlotPointFactory plotPointFactory)
    {
        Num = num;
        PlotPoints = CreatePlotPoints(plotPointFactory).ToArray();
    }

    protected abstract IEnumerable<PlotPoint> CreatePlotPoints(PlotPointFactory plotPointFactory);
}
