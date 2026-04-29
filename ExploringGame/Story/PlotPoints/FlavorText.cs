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


public class FlavorTextFactory
{
    private readonly LoadedLevelData _loadedLevelData;
    private readonly Player _player;
    private readonly IPlayerInput _playerInput;
    private readonly Physics _physics;
    private readonly DialogueManager _dialogueManager;
    private readonly PlayerActor _actor;

    public FlavorTextFactory(LoadedLevelData loadedLevelData, IPlayerInput playerInput, Player player, Physics physics, 
        DialogueManager dialogueManager, PlayerActor playerActor)
    {
        _loadedLevelData= loadedLevelData;
        _player = player;
        _playerInput = playerInput;
        _physics = physics;
        _actor = playerActor;
        _dialogueManager = dialogueManager;
    }

    public FlavorText<TShape> Create<TShape>(string text, string tag = null)
        where TShape : Shape, ICollidable
    {
        return new FlavorText<TShape>(_loadedLevelData, _playerInput, _player, _physics, _actor, _dialogueManager, text, tag);
    }
}

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

    public FlavorText(LoadedLevelData loadedLevelData, IPlayerInput playerInput, Player player, Physics physics,
        PlayerActor playerActor, DialogueManager dialogueManager,
        string text, string shapeTag, params PlotPoint[] requiredDone) 
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

    public string Text { get; }

    public string ShapeTag { get; }

    float IPlayerActivated.ActivationRange => 2.0f;

    IPlayerInput IPlayerActivated.PlayerInput => _playerInput;

    Player IPlayerActivated.Player => _player;

    ICollidable IPlayerActivated.Shape => _shape as ICollidable;

    protected override bool CheckActivation(GameTime gameTime)
    {
        return this.CheckPlayerActivation(_physics, GameKey.DialogueAdvance);
    }

    protected override void OnReady()
    {
        _shape = _loadedLevelData.ActiveSegments.SelectMany(p => p.WorldSegment.TraverseAllChildren())
            .OfType<TShape>()
            .FirstOrDefault(p => ShapeTag == null || ShapeTag == p.Tag);
    }

    protected override PlotUpdate UpdateActive(GameTime gameTime)
    {
        _dialogueManager.Enqueue(new DialogueEntry(_playerActor, Text));
        return PlotUpdate.Reset;
    }
}
