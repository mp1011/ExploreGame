using ExploringGame.Logics.Controllers.LightSpiritPhases;
using ExploringGame.Story.PlotPoints;
using ExploringGame.Story.Scene01;
using ExploringGame.Story.Scene01.Act02;
using Microsoft.Xna.Framework;
using System;
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
        
        FastForwardToAct<ActTwo>();
    }

    public void Update(GameTime gameTime)
    {
        bool nextScene = false;

        foreach (var plotPoint in CurrentAct.PlotPoints)
        {
            if (plotPoint.Update(gameTime) == PlotPointState.NextScene)
                nextScene = true;
        }

        if(nextScene)
            NextScene();
    }

    private void NextScene()
    {
        foreach (var plotPoint in CurrentAct.PlotPoints)
            plotPoint.Cleanup();

        CurrentAct = CurrentScene.Acts.Single(p => p.Num == CurrentAct.Num + 1);
    }

    public void FastForwardToAct<TAct>()
        where TAct : Act
    {
        while (CurrentAct.GetType() != typeof(TAct))
        {
            FastForwardTo<SceneFadeout>();
            NextScene();
        }
    }

    public void FastForwardTo<TAct, TPlot>()
        where TAct : Act
        where TPlot : PlotPoint
    {
        FastForwardToAct<TAct>();
        FastForwardTo<TPlot>();
    }


    public void FastForwardTo<T>()
        where T:PlotPoint
    {
        FastForwardTo(CurrentAct.PlotPoints.OfType<T>().Single());
    }

    public void FastForwardTo(PlotPoint target)
    {
        int maxTries = 1000;
        while (--maxTries > 0)
        {
            foreach (var plotPoint in CurrentAct.PlotPoints)
            {
                switch (plotPoint.Update(new GameTime()))
                {
                    case PlotPointState.Ready:
                    case PlotPointState.Active:
                        plotPoint.FastForward();
                        if (plotPoint == target)
                            return;
                        break;
                    default:
                        break;      
                }
            }
        }

        throw new Exception("Unable to fast-forward");
    }
}
