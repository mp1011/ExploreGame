using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GameDebug;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Testing;
using ExploringGame.Tests.TestHelpers;
using Xunit;

namespace ExploringGame.Tests;

public class PathfinderTest
{
    [Fact]
    public void EntityCanSeekPlayer()
    {
        var testMap = TestMaps.PathfindingTest();
        var testEntity = testMap.TraverseAllChildren().OfType<TestEntity>().First();
        testMap.PlayerStart = testMap.TraverseAllChildren().OfType<Room>().First(p => p.Tag == "Room C").LocalPosition;

        using var g = new TestGame(new SingleSegmentGroup(testMap), TimeSpan.FromMinutes(5));
        g.Run();

        var d = Debug.MovingEntityDebugger;
        var player = g.GetService<Player>();       
        Assert.True(player.LocalPosition.DistanceTo(testEntity.LocalPosition) < 3.0f);
    }
}
