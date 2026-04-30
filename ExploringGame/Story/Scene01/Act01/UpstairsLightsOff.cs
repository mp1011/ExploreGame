using ExploringGame.Entities;
using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.LevelControl;
using ExploringGame.Story.Character;
using ExploringGame.Story.PlotPoints;
using Microsoft.Xna.Framework;
using System.Linq;

namespace ExploringGame.Story.Scene01.Act01;

internal class UpstairsLightsOff : ConditionalBlocker
{
    private LightSwitch[] _switches;

    public UpstairsLightsOff(Player player, PlayerActor playerActor, DialogueManager dialogueManager, LoadedLevelData loadedLevelData) :
        base("BedroomDoorBlocker", "test 123", player, playerActor, dialogueManager, loadedLevelData)
    {
    }

    protected override void OnReady_Inner()
    {
        _switches = _loadedLevelData.LoadedSegments.FindShapes<LightSwitch>()
            .Where(p => p.StateKey == StateKey.LivingRoomLightOn || p.StateKey == StateKey.KitchenLightOn || p.StateKey == StateKey.HallLightOn)
            .ToArray();
    }

    protected override bool CheckActivation_Inner(GameTime gameTime)
    {
        return _switches.All(p => !p.Controller.On);
    }
}
