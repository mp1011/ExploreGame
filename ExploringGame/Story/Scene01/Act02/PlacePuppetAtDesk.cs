using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Services;
using ExploringGame.Story.PlotPoints;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.Story.Scene01.Act02;

public class PlacePuppetAtDesk : CharacterAction<Puppet>
{
    private LoadedLevelData _loadedLevelData;

    public PlacePuppetAtDesk(LoadedLevelData loadedLevelData, CharacterEntrance<Puppet> characterEntrance, params PlotPoint[] otherRequiredDone) : base(characterEntrance, otherRequiredDone)
    {
        _loadedLevelData = loadedLevelData;
    }

    protected override void OnActivated(Puppet shape)
    {
        var desk = _loadedLevelData.ActiveSegments.FindShape<OfficeDesk>("WifeDesk");

        shape.Active = true;
        shape.Place().At(desk);
        shape.LocalX -= 1.0f;
        shape.InitializePhysicsObject();

        shape.Controller.Mover.TargetRotation = new Rotation(0f, 0f, 0f);
        shape.Controller.Mover.AbsoluteAngularVelocity = new Vector3(4.0f, 4.0f, 4.0f);

        shape.LeftShoulder.InitializePhysicsObject();
        shape.RightShoulder.InitializePhysicsObject();
        shape.LeftArm.UpperArm.InitializePhysicsObject();
        shape.LeftArm.LowerArm.InitializePhysicsObject();
        shape.RightArm.UpperArm.InitializePhysicsObject();
        shape.RightArm.LowerArm.InitializePhysicsObject();
    }
}
