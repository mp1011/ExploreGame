using ExploringGame.Entities;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
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
        PlotPoint freeze, nar1, fadein, unfreeze, placePuppetInBed, puppetSitUp, playerAndChildPuppet, puppetCollapse, placePuppetAtDesk, playerAndWifePuppet, puppetFacePlayer;

        CharacterEntrance<Puppet> childPuppet, wifePuppet;

        #region Player Wakes Up
        yield return plotPointFactory.AmbientSound(Audio.SoundEffectKey.CreepyLoop);

        yield return freeze = plotPointFactory.Get<PlayerFreeze>();
        
        yield return plotPointFactory.PlayerMoveTo(new Vector3(-10.49f, 6.20f, 11.98f));

        yield return nar1 = plotPointFactory.Narration("What is that sound?", freeze);

        yield return fadein = plotPointFactory.Get<SceneFadein>(nar1);

        yield return unfreeze = plotPointFactory.Get<PlayerResume>(fadein);

        yield return plotPointFactory.RoomNarration("Where is that even coming from?", UpstairsHall.SouthHallTag, unfreeze);
        #endregion

        #region Child Puppet

        yield return childPuppet = plotPointFactory.CharacterEntrance<Puppet>("Child");

        yield return placePuppetInBed = plotPointFactory.CharacterAction<Puppet, PlacePuppetOnBed>(childPuppet);

        yield return puppetSitUp = plotPointFactory.CharacterAction<Puppet, PuppetSitUp>(childPuppet, placePuppetInBed);

        yield return plotPointFactory.Get<PlayerFreeze>(puppetSitUp);

        yield return plotPointFactory.LookAt<Puppet>("Child", puppetSitUp);

        yield return playerAndChildPuppet = plotPointFactory.Get<PlayerAndChildPuppet>(puppetSitUp);

        yield return plotPointFactory.Get<PlayerResume>(playerAndChildPuppet);

        yield return puppetCollapse = plotPointFactory.CharacterAction<Puppet, PuppetCollapse>(childPuppet, playerAndChildPuppet);

        yield return plotPointFactory.Narration("This has to be a dream...", puppetCollapse);

        #endregion

        #region Wife Puppet

        yield return wifePuppet = plotPointFactory.CharacterEntrance<Puppet>("Wife");
        yield return placePuppetAtDesk = plotPointFactory.CharacterAction<Puppet, PlacePuppetAtDesk>(wifePuppet);

        yield return puppetFacePlayer = plotPointFactory.CharacterAction<Puppet, PuppetFacePlayer>(wifePuppet, placePuppetAtDesk);

        yield return plotPointFactory.Get<PlayerFreeze>(puppetFacePlayer);
        yield return plotPointFactory.LookAt<Puppet>("Wife", puppetFacePlayer);

        yield return playerAndWifePuppet = plotPointFactory.Get<PlayerAndWifePuppet>(puppetFacePlayer);
        
        yield return plotPointFactory.Get<PlayerResume>(playerAndWifePuppet);

        yield return plotPointFactory.CharacterAction<Puppet, PuppetCollapse>(wifePuppet, playerAndWifePuppet);

        #endregion
    }
}
