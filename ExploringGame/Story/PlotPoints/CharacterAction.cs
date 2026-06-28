using ExploringGame.Logics;
using Microsoft.Xna.Framework;
using System.Linq;

namespace ExploringGame.Story.PlotPoints;

public abstract class CharacterAction<TShape> : PlotPoint
    where TShape : IPhysicsShape
{
    private readonly CharacterEntrance<TShape> _characterEntrance;

    public CharacterAction(CharacterEntrance<TShape> characterEntrance, params PlotPoint[] otherRequiredDone) : base(new PlotPoint[] { characterEntrance }.Union(otherRequiredDone))
    {
        _characterEntrance = characterEntrance;
    }

    protected override bool CheckActivation(GameTime gameTime) => true;

    protected override void OnActivated() => OnActivated(_characterEntrance.LoadedEntity);

    protected override PlotUpdate UpdateActive(GameTime gameTime) => UpdateActive(_characterEntrance.LoadedEntity);

    protected virtual void OnActivated(TShape shape)
    {
    }

    protected virtual PlotUpdate UpdateActive(TShape shape)
    {
        return PlotUpdate.End;
    }
}
