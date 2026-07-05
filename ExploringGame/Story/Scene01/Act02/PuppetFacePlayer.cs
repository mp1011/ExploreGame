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

public class PuppetFacePlayer : CharacterAction<Puppet>
{
    private Player _player;

    public PuppetFacePlayer(Player player, CharacterEntrance<Puppet> characterEntrance, params PlotPoint[] otherRequiredDone) : base(characterEntrance, otherRequiredDone)
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
        // todo, rotate to face player
        shape.Controller.Mover.AbsoluteAngularVelocity = new Vector3(4.0f, 4.0f, 4.0f);
    }

    protected override PlotUpdate UpdateActive(Puppet shape)
    {
        return PlotUpdate.End;
    }
}
