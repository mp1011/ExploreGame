using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Logics.Collision;
using ExploringGame.Services;
using ExploringGame.Story.Character;
using ExploringGame.Story.PlotPoints;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Story;

public enum PlotPointState
{
    Idle,
    Ready,
    Active,
    Done,
    NextScene
}

public enum PlotUpdate
{
    Continue,
    End,
    Reset,
    NextScene
}

public static class PlotPointExtensions
{
    public static IEnumerable<PlotPoint> ByState(this IEnumerable<PlotPoint> points, PlotPointState state) =>
        points.Where(p => p.State == state);
}

public class PlotPointFactory
{
    private readonly ServiceContainer _serviceContainer;
    private readonly LoadedLevelData _loadedLevelData;
    private readonly Player _player;
    private readonly IPlayerInput _playerInput;
    private readonly Physics _physics;
    private readonly DialogueManager _dialogueManager;
    private readonly PlayerActor _actor;
    private readonly CameraService _cameraService;

    public PlotPointFactory(ServiceContainer serviceContainer, LoadedLevelData loadedLevelData, IPlayerInput playerInput, Player player, 
        Physics physics, DialogueManager dialogueManager, PlayerActor playerActor, CameraService cameraService)
    {
        _serviceContainer = serviceContainer;
        _cameraService = cameraService;
        _loadedLevelData = loadedLevelData;
        _player = player;
        _playerInput = playerInput;
        _physics = physics;
        _actor = playerActor;
        _dialogueManager = dialogueManager;
    }

    public FlavorText<TShape> FlavorText<TShape>(string text, string tag = null)
        where TShape : Shape, ICollidable
    {
        return new FlavorText<TShape>(_loadedLevelData, _playerInput, _player, _physics, _actor, _dialogueManager, text, tag);
    }

    public Narration Narration(string text, params PlotPoint[] requiredDone)
    {
        return new Narration(_loadedLevelData, _actor, _dialogueManager, text, requiredDone);
    }

    public CameraLookAt<TShape> LookAt<TShape>(string tag = null, params PlotPoint[] requiredDone)
        where TShape : Shape
    {
        return new CameraLookAt<TShape>(tag, _loadedLevelData, _cameraService, requiredDone);
    }

    public DoorBlocker DoorBlocker(Dictionary<StateKey, string> blockMessages, params PlotPoint[] requiredDone)
    {
        return new DoorBlocker(blockMessages, _player, _actor, _dialogueManager, _loadedLevelData, _playerInput, _physics, requiredDone);
    }

    public SwitchBlocker SwitchBlocker(Dictionary<StateKey, string> blockMessages, params PlotPoint[] requiredDone)
    {
        return new SwitchBlocker(blockMessages, _player, _actor, _dialogueManager, _loadedLevelData, _playerInput, _physics, requiredDone);
    }

    public SwitchChanged SwitchChanged(StateKey key, bool targetState, params PlotPoint[] requiredDone)
    {
        return new SwitchChanged(_loadedLevelData, key, targetState, requiredDone);
    }

    public T Get<T>(params PlotPoint[] requiredDone) where T:PlotPoint
    {
        return _serviceContainer.Get<T>(new Ninject.Parameters.ConstructorArgument("requiredDone", requiredDone));
    }
}

public abstract class PlotPoint
{
    public PlotPointState State { get; private set; }

    public PlotPoint[] RequiredDone { get; }

    protected TimeSpan? _activationTime;

    public PlotPoint(IEnumerable<PlotPoint> requiredDone)
    {
        RequiredDone = requiredDone.ToArray();
    }

    public PlotPointState Update(GameTime gameTime)
    {
        State = UpdateState(gameTime);
        return State;
    }

    private PlotPointState UpdateState(GameTime gameTime)
    {
        switch (State)
        {
            case PlotPointState.Idle:
                if (RequiredDone.Any(p => p.State != PlotPointState.Done))
                    return PlotPointState.Idle;
                else
                {
                    OnReady();
                    return PlotPointState.Ready;
                }
            case PlotPointState.Ready:
                if (CheckActivation(gameTime))
                {
                    OnActivated();
                    _activationTime = gameTime.TotalGameTime;
                    return PlotPointState.Active;
                }
                else
                    return PlotPointState.Ready;
            case PlotPointState.Active:
                var updateResult = UpdateActive(gameTime);
                if (updateResult == PlotUpdate.End)
                    return PlotPointState.Done;
                else if (updateResult == PlotUpdate.Reset)
                    return PlotPointState.Ready;
                else if (updateResult == PlotUpdate.NextScene)
                    return PlotPointState.NextScene;
                else
                    return PlotPointState.Active;
            default:
                return State;
        }
    }

    protected virtual void OnReady()
    {

    }

    protected virtual void OnActivated()
    {

    }

    protected abstract bool CheckActivation(GameTime gameTime);

    protected abstract PlotUpdate UpdateActive(GameTime gameTime);
}
