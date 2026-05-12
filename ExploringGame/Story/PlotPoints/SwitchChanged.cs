using ExploringGame.LevelControl;
using ExploringGame.Logics.ShapeControllers;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace ExploringGame.Story.PlotPoints;

/// <summary>
/// Finishes when the given switch is changed
/// </summary>
public class SwitchChanged : PlotPoint
{
    private readonly LoadedLevelData _loadedLevelData;
    private StateKey _key;
    private bool _targetState;

    private ISwitchShape _switch;

    public SwitchChanged(LoadedLevelData loadedLevelData, StateKey key, bool targetState, params PlotPoint[] requiredDone)
        :base(requiredDone)
    {
        _loadedLevelData = loadedLevelData;
        _key = key;
        _targetState = targetState;
    }

    protected override void OnReady()
    {
        _switch = _loadedLevelData.LoadedSegments.SelectMany(p => p.WorldSegment.TraverseAllChildren())
            .OfType<ISwitchShape>()
            .Single(p => p.StateKey == _key);
    }

    protected override bool CheckActivation(GameTime gameTime)
    {
        return _switch.On == _targetState;
    }

    protected override PlotUpdate UpdateActive(GameTime gameTime)
    {
        return PlotUpdate.End;
    }

    protected override void FastForward_Inner()
    {
        if (State < PlotPointState.Ready)
            OnReady();
        _switch.On = _targetState;
    }
}
