using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Services;
using ExploringGame.Story.PlotPoints;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.Story.Scene01.Act02;

public class PuppetSitUp : CharacterAction<Puppet>
{
    private Player _player;

    public PuppetSitUp(Player player, CharacterEntrance<Puppet> characterEntrance, params PlotPoint[] otherRequiredDone) : base(characterEntrance, otherRequiredDone)
    {
        _player = player;
    }

    protected override bool CheckActivation(GameTime gameTime, Puppet shape)
    {
        var dist = _player.WorldPosition.DistanceTo(shape.WorldPosition);
        return dist < Measure.Feet(7);
    }

    protected override void OnActivated(Puppet shape)
    {
        shape.Controller.Mover.TargetRotation = new Rotation(0f, 0f, 0f);
        shape.Controller.Mover.AbsoluteAngularVelocity = new Vector3(6.0f, 0f, 0f);
    }

    protected override PlotUpdate UpdateActive(Puppet shape)
    {
        var r = new Rotation(shape.ColliderBodies[0].Orientation.ToQuaternion());
        var r2 = shape.Controller.Mover.TargetRotation;

        GameDebug.Debug.Watch1 = $"Rotation: {r}";
        GameDebug.Debug.Watch2 = $"Target: {r2}";

        return PlotUpdate.Continue;
    }
}
