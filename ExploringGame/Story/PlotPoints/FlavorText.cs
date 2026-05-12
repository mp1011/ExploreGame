using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Logics.Collision;
using ExploringGame.Logics.ShapeControllers;
using ExploringGame.Services;
using ExploringGame.Story.Character;
using Microsoft.Xna.Framework;
using System.Linq;

namespace ExploringGame.Story.PlotPoints;

public class FlavorText<TShape> : PlotPoint, IPlayerActivated
    where TShape : Shape, ICollidable
{
    private readonly PlayerActor _playerActor;
    private readonly DialogueManager _dialogueManager;
    private readonly LoadedLevelData _loadedLevelData;
    private readonly Physics _physics;
    private readonly IPlayerInput _playerInput;
    private readonly Player _player;
    private Shape _shape;
    private GameKey _activationKey;

    public FlavorText(LoadedLevelData loadedLevelData, IPlayerInput playerInput, Player player, Physics physics,
        PlayerActor playerActor, DialogueManager dialogueManager,
        string text, string shapeTag, GameKey activationKey = GameKey.DialogueAdvance, params PlotPoint[] requiredDone) 
        : base(requiredDone)
    {
        _loadedLevelData = loadedLevelData;
        _player = player;
        _playerInput = playerInput;
        _physics = physics;
        _playerActor = playerActor;
        _dialogueManager = dialogueManager;
        ShapeTag = shapeTag;
        Text = text;
    }

    public FlavorText(LoadedLevelData loadedLevelData, IPlayerInput playerInput, Player player, Physics physics,
       PlayerActor playerActor, DialogueManager dialogueManager,
       string text, Shape shape, GameKey activationKey = GameKey.DialogueAdvance, params PlotPoint[] requiredDone)
       : base(requiredDone)
    {
        _loadedLevelData = loadedLevelData;
        _player = player;
        _playerInput = playerInput;
        _physics = physics;
        _playerActor = playerActor;
        _dialogueManager = dialogueManager;
        ShapeTag = string.Empty;
        _shape = shape;
        _activationKey = activationKey;
        Text = text;
    }

    public string Text { get; }

    public string ShapeTag { get; }

    float IPlayerActivated.ActivationRange => 2.0f;

    IPlayerInput IPlayerActivated.PlayerInput => _playerInput;

    Player IPlayerActivated.Player => _player;

    ICollidable IPlayerActivated.Shape => _shape as ICollidable;

    protected override bool CheckActivation(GameTime gameTime)
    {
        return this.CheckPlayerActivation(_physics, _activationKey);
    }

    protected override void OnReady()
    {
        if (_shape != null)
            return;

        _shape = _loadedLevelData.ActiveSegments.SelectMany(p => p.WorldSegment.TraverseAllChildren())
            .OfType<TShape>()
            .Single(p => ShapeTag == null || ShapeTag == p.Tag);
    }

    protected override PlotUpdate UpdateActive(GameTime gameTime)
    {
        _dialogueManager.EnqueueIfNeeded(new DialogueEntry(_playerActor, Text));
        return PlotUpdate.Reset;
    }

    public override void Cleanup()
    {
        _dialogueManager.Remove(_playerActor, Text);
    }

}
