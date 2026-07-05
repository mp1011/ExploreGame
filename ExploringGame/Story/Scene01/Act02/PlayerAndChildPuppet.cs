using ExploringGame.LevelControl;
using ExploringGame.Story.Character;
using ExploringGame.Story.PlotPoints;
using System.Collections.Generic;

namespace ExploringGame.Story.Scene01.Act02;

internal class PlayerAndChildPuppet : Dialogue
{
    public PlayerAndChildPuppet(LoadedLevelData loadedLevelData, PlayerActor playerActor, ChildPuppet otherActor, DialogueManager dialogueManager, params PlotPoint[] requiredDone) 
        : base(loadedLevelData, playerActor, otherActor, dialogueManager, requiredDone)
    {
    }

    protected override IEnumerable<DialogueEntry> CreateEntries(PlayerActor playerActor, StoryActor otherActor)
    {
        yield return new DialogueEntry(otherActor, "Daddy!");
        yield return new DialogueEntry(playerActor, "What the hell? This is not my child!");
    }
}
