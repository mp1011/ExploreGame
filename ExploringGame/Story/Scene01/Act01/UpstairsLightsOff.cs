using ExploringGame.Entities;
using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
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
        base("BedroomDoorBlocker", "I still have to turn off more lights", player, playerActor, dialogueManager, loadedLevelData)
    {
    }

    protected override void OnReady_Inner()
    {
        _switches = _loadedLevelData.LoadedSegments.FindShapes<LightSwitch>()
            .Where(p => p.StateKey == StateKey.LivingRoomLightOn || p.StateKey == StateKey.KitchenLightOn || p.StateKey == StateKey.HallLightOn)
            .ToArray();

        foreach (var lightSwitch in _switches)
            lightSwitch.Controller.On = true;

        var bedroomLamp = _loadedLevelData.LoadedSegments.FindShapes<Lamp>().Single(p => p.StateKey == StateKey.LeftBedroomLightOn);
        bedroomLamp.Controller.On = true;

    }

    protected override bool CheckActivation_Inner(GameTime gameTime)
    {
        return _switches.All(p => !p.Controller.On);
    }
}
