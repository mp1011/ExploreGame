using ExploringGame.Entities;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Services;
using ExploringGame.Story.PlotPoints;

namespace ExploringGame.Story.Scene01.Act02;

public class PlacePuppetOnBed : CharacterAction<Puppet>
{
    private LoadedLevelData _loadedLevelData;

    public PlacePuppetOnBed(LoadedLevelData loadedLevelData, CharacterEntrance<Puppet> characterEntrance, params PlotPoint[] otherRequiredDone) : base(characterEntrance, otherRequiredDone)
    {
        _loadedLevelData = loadedLevelData;
    }

    protected override void OnActivated(Puppet shape)
    {
        var room = _loadedLevelData.ActiveSegments.FindShape<KidsBedroom>();
        var bed = room.FindChild<SmallBed>();

        shape.Place().At(bed).OnFloor();
        shape.InitializePhysicsObject();
    }
}
