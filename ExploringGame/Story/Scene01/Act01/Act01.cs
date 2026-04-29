using ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;
using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.LevelControl;
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
        var flavorTextFactory = serviceContainer.Get<FlavorTextFactory>();

        yield return serviceContainer.Get<DebugMessage>();        
        yield return flavorTextFactory.Create<Door>(tag: "FrontDoor", text: "hello world");
    }
}
