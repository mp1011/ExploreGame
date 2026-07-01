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
       

    }

    protected override PlotUpdate UpdateActive(Puppet shape)
    {
        shape.ColliderBodies[0].AngularVelocity = new Jitter2.LinearMath.JVector(-2.5f, 0f, 0f);
        return PlotUpdate.Continue;
    }
}
