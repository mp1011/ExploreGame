using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using System;
using System.Collections.Generic;

namespace ExploringGame.Story;

public class NullScene : Scene
{
    public NullScene(HomeWorldSegmentGroup worldSegmentGroup, NullAct nullAct) : base(worldSegmentGroup, nullAct)
    {
    }
}

public class NullAct : Act
{
    public NullAct(PlotPointFactory plotPointFactory) : base(0, plotPointFactory)
    {
    }

    protected override IEnumerable<PlotPoint> CreatePlotPoints(PlotPointFactory plotPointFactory)
    {
        return Array.Empty<PlotPoint>();
    }
}
