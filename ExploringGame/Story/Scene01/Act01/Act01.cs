using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.LevelControl;
using ExploringGame.Story.PlotPoints;
using System.Collections.Generic;

namespace ExploringGame.Story.Scene01.Act01;

public class ActOne : Act
{
    public ActOne(PlotPointFactory plotPointFactory) : base(1, plotPointFactory)
    {
    }

    protected override IEnumerable<PlotPoint> CreatePlotPoints(PlotPointFactory plotPointFactory)
    {
        PlotPoint nar1, nar2, lookAtHallLight;

        yield return plotPointFactory.Get<PlayerFreeze>();

        yield return nar1 = plotPointFactory.Narration("Time to get some sleep");
        yield return nar2 = plotPointFactory.Narration("I can't forget to turn off the lights");

        yield return lookAtHallLight = plotPointFactory.LookAt<LightSwitch>(UpstairsHall.LightSwitchTag, nar1);

        yield return plotPointFactory.FlavorText<Door>(tag: "FrontDoor", text: "hello world");
        yield return plotPointFactory.Get<UpstairsLightsOff>();
        yield return plotPointFactory.Get<PlayerResume>(lookAtHallLight, nar2);
    }
}
