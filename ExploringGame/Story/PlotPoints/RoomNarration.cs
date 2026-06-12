using ExploringGame.Entities;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.LevelControl;
using ExploringGame.Story.Character;
using Microsoft.Xna.Framework;

namespace ExploringGame.Story.PlotPoints;

/// <summary>
/// Occurs when player enters a room
/// </summary>
internal class RoomNarration<TRoom> : Narration
    where TRoom:IRoom
{
    private readonly Player _player;
    private TRoom _room;
    private string _tag;

    public RoomNarration(LoadedLevelData loadedLevelData, Player player, PlayerActor playerActor, DialogueManager dialogueManager, string text,
        string tag,
        params PlotPoint[] requiredDone) : base(loadedLevelData, playerActor, dialogueManager, text, requiredDone)
    {
        _player = player;
        _tag = tag;
    }

    protected override void OnReady()
    {
        _room = _loadedLevelData.ActiveSegments.FindShape<TRoom>(_tag);
    }

    protected override bool CheckActivation(GameTime gameTime)
    {
        return _room.ContainsPoint(_player.WorldPosition);
    }
}
