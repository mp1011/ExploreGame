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
        PlotPoint nar1, nar2, nar3, lookAtHallLight, upstairsLightsOff, bedroomLampOff, lieInBed;

        yield return plotPointFactory.DoorBlocker(
            new()
            {
                [StateKey.KidsBedroomDoorOpen] = "And wake the kid? Not a chance.",
                [StateKey.BathroomDoorOpen] = "I don't need to go in there",
                [StateKey.FrontDoorOpen] = "I don't need to go outside",
                [StateKey.DenDoorsOpen] = "I don't need to go in there",
                [StateKey.SpareRoomDoorOpen] = "I don't need to go in there",
                [StateKey.LinenClosetDoorOpen] = "Now is not the time for a towel",
                [StateKey.BasementStairsDoorOpen] = "My wife is down there playing her games",
            });

        yield return plotPointFactory.Get<PlayerFreeze>();

        yield return nar1 = plotPointFactory.Narration("Time to get some sleep");
        yield return nar2 = plotPointFactory.Narration("I can't forget to turn off the lights");

        yield return lookAtHallLight = plotPointFactory.LookAt<LightSwitch>(UpstairsHall.LightSwitchTag, nar1);

        yield return upstairsLightsOff = plotPointFactory.Get<UpstairsLightsOff>();
        yield return plotPointFactory.Get<PlayerResume>(lookAtHallLight, nar2);

        yield return plotPointFactory.SwitchBlocker(
            new()
            {
                [StateKey.HallLightOn] = "This should stay off",
                [StateKey.KitchenLightOn] = "This should stay off",
                [StateKey.LivingRoomLightOn] = "This should stay off",
                [StateKey.RightBedroomLightOn] = "This should stay off",
            }, upstairsLightsOff);

        yield return bedroomLampOff = plotPointFactory.SwitchChanged(StateKey.LeftBedroomLightOn, targetState: false);
        yield return plotPointFactory.Get<PlayerFreeze>(bedroomLampOff);

        yield return nar3 = plotPointFactory.Narration("Finally, sleep", bedroomLampOff);

        yield return lieInBed = plotPointFactory.Get<LieInBed>(nar3);
    }
}
