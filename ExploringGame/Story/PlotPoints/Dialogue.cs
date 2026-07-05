using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Story.Character;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Story.PlotPoints;

public abstract class Dialogue : PlotPoint
{
    protected readonly PlayerActor _playerActor;
    protected readonly StoryActor _otherActor;
    private readonly DialogueManager _dialogueManager;
    protected readonly LoadedLevelData _loadedLevelData;

    private DialogueEntry[] _entries;

    public Dialogue(LoadedLevelData loadedLevelData, PlayerActor playerActor, StoryActor otherActor, DialogueManager dialogueManager,
        params PlotPoint[] requiredDone)
        : base(requiredDone)
    {
        _loadedLevelData = loadedLevelData;
        _playerActor = playerActor;
        _otherActor = otherActor;
        _dialogueManager = dialogueManager;
    }

    protected override bool CheckActivation(GameTime gameTime) => true;

    protected abstract IEnumerable<DialogueEntry> CreateEntries(PlayerActor playerActor, StoryActor otherActor);

    protected override void OnActivated()
    {
        _entries = CreateEntries(_playerActor, _otherActor).ToArray();
        foreach (var entry in _entries)
            _dialogueManager.Enqueue(entry);
    }

    protected override PlotUpdate UpdateActive(GameTime gameTime)
    {       
        if (_dialogueManager.Current != null)
            return PlotUpdate.Continue;
        else 
            return PlotUpdate.End;
    }

    public override void Cleanup()
    {
        foreach(var entry in _entries)
            _dialogueManager.Remove(_playerActor, entry.Line);
    }
}
