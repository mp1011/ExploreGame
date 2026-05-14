using ExploringGame.Entities;
using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using ExploringGame.LevelControl;
using ExploringGame.Logics.Controllers;
using ExploringGame.Services;
using ExploringGame.Story.PlotPoints;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.Story.Scene01.Act01;

public class LieInBed : PlotPoint
{
    private CameraLookAt<CeilingVent> _cameraLookAt;
    private LoadedLevelData _loadedLevelData;
    private Player _player;
    private Bed _bed;

    private TimedAction _fadeOut;

    public LieInBed(PlotPointFactory factory, Player player, LoadedLevelData loadedLevelData,
        params PlotPoint[] requiredDone) : base(requiredDone)
    {
        _cameraLookAt = factory.LookAt<CeilingVent>(Bedroom.VentTag);
        _loadedLevelData = loadedLevelData;
        _player = player;
    }

    protected override void OnReady()
    {
        _bed = _loadedLevelData.ActiveSegments.FindShape<Bed>();
    }

    protected override bool CheckActivation(GameTime gameTime) => true;

    protected override PlotUpdate UpdateActive(GameTime gameTime)
    {
        _player.Position = _bed.Position;

        _cameraLookAt.Update(gameTime);

        if ((gameTime.TotalGameTime - _activationTime) > TimeSpan.FromSeconds(3))
            return PlotUpdate.End;
        else
            return PlotUpdate.Continue;
    }

    protected override PlotUpdate FastForward_Inner()
    {
        if (State < PlotPointState.Ready)
            OnReady();
        _player.Position = _bed.Position;
        return PlotUpdate.End;
    }
}
