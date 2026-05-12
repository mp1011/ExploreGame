using ExploringGame.Entities;
using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.LevelControl;
using ExploringGame.Logics.Collision;
using ExploringGame.Story.Character;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Story.PlotPoints;

public abstract class ConditionalBlocker : PlotPoint
{
    protected readonly LoadedLevelData _loadedLevelData;
    private readonly PlayerActor _playerActor;
    private readonly Player _player;
    private readonly DialogueManager _dialogueManager;
    private string _blockerTag;
    private string _messageWhenBlocked;
    private Blocker _blocker;
    private CollidesWithShape _blockerCollision;


    public ConditionalBlocker(string blockerTag, string messageWhenBlocked, Player player, PlayerActor playerActor, DialogueManager dialogueManager, LoadedLevelData loadedLevelData, 
        params PlotPoint[] requiredDone) : base(requiredDone)
    {
        _player = player;
        _blockerTag = blockerTag;
        _loadedLevelData = loadedLevelData;
        _dialogueManager = dialogueManager;
        _playerActor = playerActor;
        _messageWhenBlocked = messageWhenBlocked;
    }

    protected sealed override void OnReady()
    {
        _blocker = _loadedLevelData.ActiveSegments.FindShape<Blocker>(_blockerTag);
        _blocker.Enabled = true;
        _blockerCollision = _player.Mover.CollisionResponder.AddResponse(new CollidesWithShape(_blocker));
        _blockerCollision.CollisionOccured += BlockerCollision_CollisionOccurred;
        OnReady_Inner();
    }

    private void BlockerCollision_CollisionOccurred(object sender, System.EventArgs e)
    {
        _dialogueManager.EnqueueIfNeeded(new DialogueEntry(_playerActor, _messageWhenBlocked));
    }

    protected virtual void OnReady_Inner()
    {

    }

    protected sealed override bool CheckActivation(GameTime gameTime)
    {
        bool ready = CheckActivation_Inner(gameTime);


        return ready;
    }

    protected virtual bool CheckActivation_Inner(GameTime gameTime)
    {
        return false;
    }

    protected override PlotUpdate UpdateActive(GameTime gameTime)
    {
        _blocker.Enabled = false;
        return PlotUpdate.End;
    }

    protected override void FastForward_Inner()
    {
        if (State < PlotPointState.Ready)
            OnReady();

        _blocker.Enabled = false;
    }
}
