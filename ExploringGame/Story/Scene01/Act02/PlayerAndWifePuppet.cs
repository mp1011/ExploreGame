using ExploringGame.LevelControl;
using ExploringGame.Story.Character;
using ExploringGame.Story.PlotPoints;
using System.Collections.Generic;

namespace ExploringGame.Story.Scene01.Act02;

internal class PlayerAndWifePuppet : Dialogue
{
    public PlayerAndWifePuppet(LoadedLevelData loadedLevelData, PlayerActor playerActor, WifePuppet otherActor, DialogueManager dialogueManager, params PlotPoint[] requiredDone) 
        : base(loadedLevelData, playerActor, otherActor, dialogueManager, requiredDone)
    {
    }

    protected override IEnumerable<DialogueEntry> CreateEntries(PlayerActor playerActor, StoryActor otherActor)
    {
        yield return new DialogueEntry(playerActor, "Honey, do you hear that noise?");

        yield return new DialogueEntry(otherActor, "No, what noise?");

        yield return new DialogueEntry(playerActor, "What the hell? You are not my wife!");

        yield return new DialogueEntry(otherActor, "I don't understand, what do you mean?");

    }
}
