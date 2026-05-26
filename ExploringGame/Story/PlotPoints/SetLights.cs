using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.LevelControl;
using ExploringGame.Logics.ShapeControllers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExploringGame.Story.PlotPoints;

public class SetLights : PlotPoint
{
    private StateKey[] _lights;
    private LoadedLevelData _loadedLevelData;


    public SetLights(LoadedLevelData loadedLevelData, params StateKey[] lights) : base(Array.Empty<PlotPoint>())
    {
        _lights = lights;
        _loadedLevelData = loadedLevelData;
    }

    protected override void OnReady()
    {
        foreach(var switchShape in _loadedLevelData.LoadedSegments.FindShapes<ISwitchShape>())
        {
            switchShape.Controller.On = _lights.Contains(switchShape.StateKey);
        }


        base.OnReady();
    }
    protected override bool CheckActivation(GameTime gameTime) => true;

    protected override PlotUpdate UpdateActive(GameTime gameTime) => PlotUpdate.End;
}
