using ExploringGame.Entities;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using ExploringGame.Story.PlotPoints;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ExploringGame.Story.Scene01.Act02;

public class ActTwo : Act
{
    public ActTwo(PlotPointFactory plotPointFactory) : base(2, plotPointFactory)
    {
    }

    protected override IEnumerable<PlotPoint> CreatePlotPoints(PlotPointFactory plotPointFactory)
    {
        PlotPoint freeze, nar1, fadein, unfreeze;

        yield return plotPointFactory.AmbientSound(Audio.SoundEffectKey.CreepyLoop);

        yield return freeze = plotPointFactory.Get<PlayerFreeze>();
        yield return plotPointFactory.PlayerMoveTo(new Vector3(-10.49f, 6.20f, 11.98f));

        yield return nar1 = plotPointFactory.Narration("What is that sound?", freeze);

        yield return fadein = plotPointFactory.Get<SceneFadein>(nar1);

        yield return unfreeze = plotPointFactory.Get<PlayerResume>(fadein);

        yield return plotPointFactory.RoomNarration("Where is that even coming from?", UpstairsHall.SouthHallTag, unfreeze);

        yield return plotPointFactory.PlaceObject<Puppet, KidsBedroom>("Child", Vector3.Zero);
    }
}
