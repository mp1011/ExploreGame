using ExploringGame.LevelControl;
using ExploringGame.Logics;
using Microsoft.Xna.Framework;

namespace ExploringGame.Story.PlotPoints;

public abstract class CharacterEntrance
{
    public IPhysicsShape Shape { get; set; }
}

public class CharacterEntrance<TEntity> : PlotPoint
    where TEntity : IPhysicsShape
{
    public TEntity LoadedEntity { get; private set; }

    private readonly LoadedLevelData _loadedLevelData;

    public CharacterEntrance(LoadedLevelData loadedLevelData, params PlotPoint[] requiredDone) : base(requiredDone)
    {
        _loadedLevelData = loadedLevelData;
    }

    protected override void OnReady()
    {
        LoadedEntity = _loadedLevelData.LoadedSegments.FindShape<TEntity>();
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
