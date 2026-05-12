using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Logics.Collision;
using ExploringGame.Services;
using ExploringGame.Story.Character;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ExploringGame.Story.PlotPoints;

/// <summary>
/// Prevents the user from going through certain doors
/// </summary>
public class DoorBlocker : PlotPoint
{
    private readonly Dictionary<StateKey, string> _blockMessages;
    private readonly DialogueManager _dialogueManager;
    private readonly PlayerActor _playerActor;
    private readonly LoadedLevelData _loadedLevelData;
    private readonly Player _player;
    private readonly IPlayerInput _playerInput;
    private readonly Physics _physics;

    private List<FlavorText<Blocker>> _flavorText = new();

    public DoorBlocker(Dictionary<StateKey, string> blockMessages, Player player, PlayerActor playerActor, DialogueManager dialogueManager, 
        LoadedLevelData loadedLevelData, IPlayerInput playerInput, Physics physics, params PlotPoint[] requiredDone) : base(requiredDone)
    {
        _player = player;
        _physics = physics;
        _playerInput= playerInput;
        _blockMessages = blockMessages;
        _loadedLevelData = loadedLevelData;
        _playerActor = playerActor;
        _dialogueManager = dialogueManager;
    }

    protected override void OnReady()
    {
        foreach (var entry in _blockMessages)
            SetupBlocker(entry.Key, entry.Value);
    }

    private void SetupBlocker(StateKey key, string message)
    {
        var doorJunction = _loadedLevelData.ActiveSegments.FindShapes<Door>().Where(p => p.StateKey == key)
            .Select(p => p.Parent)
            .Distinct()
            .Single();

        var blocker = _loadedLevelData.ActiveSegments.FindShapes<Blocker>().First(p => p.BlockingShape == doorJunction);
        blocker.Enabled = true;

        _flavorText.Add(new FlavorText<Blocker>(_loadedLevelData, _playerInput, _player, _physics, _playerActor, _dialogueManager,
            message, blocker, GameKey.Use));
    }


    protected override bool CheckActivation(GameTime gameTime) => true;

    protected override PlotUpdate UpdateActive(GameTime gameTime)
    {
        foreach (var f in _flavorText)
            f.Update(gameTime);

        return PlotUpdate.Continue;
    }

    public override void Cleanup()
    {
        foreach (var entry in _blockMessages)
        {
            var doorJunction = _loadedLevelData.ActiveSegments.FindShapes<Door>().Where(p => p.StateKey == entry.Key)
           .Select(p => p.Parent)
           .Distinct()
           .Single();

            var blocker = _loadedLevelData.ActiveSegments.FindShapes<Blocker>().First(p => p.BlockingShape == doorJunction);
            blocker.Enabled = false;
        }
    }
}
