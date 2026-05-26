using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Story.Character;
using Microsoft.Xna.Framework;

namespace ExploringGame.Story.PlotPoints;

public class Narration : PlotPoint
{
    protected readonly PlayerActor _playerActor;
    private readonly DialogueManager _dialogueManager;
    protected readonly LoadedLevelData _loadedLevelData;
 
    public Narration(LoadedLevelData loadedLevelData, PlayerActor playerActor, DialogueManager dialogueManager,
        string text, params PlotPoint[] requiredDone)
        : base(requiredDone)
    {
        _loadedLevelData = loadedLevelData;
        _playerActor = playerActor;
        _dialogueManager = dialogueManager;
        Text = text;
    }

    public string Text { get; }

    protected override bool CheckActivation(GameTime gameTime) => true;

    protected override void OnActivated()
    {
        _dialogueManager.Enqueue(new DialogueEntry(_playerActor, Text));
    }

    protected override PlotUpdate UpdateActive(GameTime gameTime)
    {       
        if (_dialogueManager.HasText(Text))
            return PlotUpdate.Continue;
        else 
            return PlotUpdate.End;
    }

    public override void Cleanup()
    {
        _dialogueManager.Remove(_playerActor, Text);
    }
}
