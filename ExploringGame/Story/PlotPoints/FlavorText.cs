using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ExploringGame.Story.PlotPoints;

public class FlavorText : PlotPoint
{
    public FlavorText(params PlotPoint[] requiredDone) : base(requiredDone)
    {
    }

    public string Text { get; }
    public Shape Shape { get; }

    public override PlotUpdate UpdateActive(GameTime gameTime)
    {
        throw new System.NotImplementedException();
    }
}
