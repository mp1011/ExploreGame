using ExploringGame.Story.Scene01;
using Microsoft.Xna.Framework;
using System.Linq;

namespace ExploringGame.Story;

public class SceneManager
{
    public Scene CurrentScene { get; private set; }
    public Act CurrentAct { get; private set; }

    public SceneManager()
    {
    }

    public void Initialize(Scene initialScene)
    {
        CurrentScene = initialScene;
        CurrentAct = initialScene.Acts.First();
    }

    public void Update(GameTime gameTime)
    {
        foreach (var plotPoint in CurrentAct.PlotPoints)
            plotPoint.Update(gameTime);
    }

}
