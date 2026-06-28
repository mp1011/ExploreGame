using ExploringGame.LevelControl;
using ExploringGame.Logics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ExploringGame.Story.PlotPoints;

public abstract class CharacterEntrance : PlotPoint
{
    protected CharacterEntrance(IEnumerable<PlotPoint> requiredDone) : base(requiredDone)
    {
    }

    public IPhysicsShape Shape { get; set; }
}

public class CharacterEntrance<TEntity> : CharacterEntrance
    where TEntity : IPhysicsShape
{
    private string _tag;

    public TEntity LoadedEntity { get; private set; }

    private readonly LoadedLevelData _loadedLevelData;

    public CharacterEntrance(string tag, LoadedLevelData loadedLevelData, params PlotPoint[] requiredDone) : base(requiredDone)
    {
        _loadedLevelData = loadedLevelData;
        _tag = tag;
    }

    protected override void OnReady()
    {
        LoadedEntity = _loadedLevelData.LoadedSegments.FindShape<TEntity>(_tag);
    }

    protected override bool CheckActivation(GameTime gameTime)
    {
        return true;
    }

    protected override PlotUpdate UpdateActive(GameTime gameTime)
    {
        return PlotUpdate.End;
    }
}
