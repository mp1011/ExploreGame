using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Logics.Pathfinding;
using ExploringGame.Tests.TestHelpers;
using System.Linq;
using System.Net.WebSockets;
using Xunit;

namespace ExploringGame.Tests;

public class RoomLightingTests
{
    [Fact]
    public void RoomWithLightSourceHasFullIntensity()
    {

        // Act - Turn off all lights, then turn on only BasementOffice lights
        bool lightsConfigured = false;
        Room basementOffice = null;

        using var game = new TestGame(new HomeWorldSegmentGroup(), framesToRun: 100, testAssertion: (g, gameTime) =>
        {
            var loadedLevelData = g.GetService<LoadedLevelData>();
            var basement = loadedLevelData.ActiveSegments.Select(p => p.WorldSegment).OfType<BasementWorldSegment>().Single();

            basementOffice = basement.TraverseAllChildren().OfType<BasementOffice>().First();

            // On first update, configure lights
            if (gameTime.TotalGameTime.TotalMilliseconds < 50)
            {
                g.SetAllLights(l=> l.Room is BasementOffice);

                lightsConfigured = true;
                return TestResult.CONTINUE;
            }

            // After lights are configured, check the levels
            if (lightsConfigured && gameTime.TotalGameTime.TotalMilliseconds > 200)
            {
                if (loadedLevelData.LightingCalculator.RoomLightGraph.TryGet(basementOffice, out var lightData))
                {
                    var lightLevel = lightData.GetTotalLight();

                    // BasementOffice should have light from its sources
                    Assert.True(lightLevel >= LightIntensity.IndoorLight, $"Expected BasementOffice light level >= {LightIntensity.IndoorLight}, got {lightLevel}");
                }

                return TestResult.PASS;
            }

            return TestResult.CONTINUE;
        });

        game.Run();
    }

    [Fact]
    public void AdjacentRoomReceivesDecayedLight()
    {

        // Act - Turn on only BasementOffice lights, check Basement receives light
        bool lightsConfigured = false;
        Room basementOffice = null;
        Room basementRoom = null;

        using var game = new TestGame(new HomeWorldSegmentGroup(), framesToRun: 100, testAssertion: (g, gameTime) =>
        {
            var loadedLevelData = g.GetService<LoadedLevelData>();
            var basement = loadedLevelData.ActiveSegments.Select(p => p.WorldSegment).OfType<BasementWorldSegment>().Single();

            // On first update, configure lights
            if (gameTime.TotalGameTime.TotalMilliseconds < 50)
            {
                g.SetAllLights(l => l.Room is BasementOffice);

                basementOffice = basement.TraverseAllChildren().OfType<BasementOffice>().First();
                basementRoom = basement.TraverseAllChildren().OfType<Basement>().First();

                lightsConfigured = true;
                return TestResult.CONTINUE;
            }

            // After lights are configured, check the light levels
            if (lightsConfigured && gameTime.TotalGameTime.TotalMilliseconds > 200)
            {
                var lightCalc = loadedLevelData.LightingCalculator;

                lightCalc.RoomLightGraph.TryGet(basementOffice, out var officeLightData);
                lightCalc.RoomLightGraph.TryGet(basementRoom, out var basementLightData);

                var officeLight = officeLightData?.GetTotalLight() ?? 0f;
                var basementLight = basementLightData?.GetTotalLight() ?? 0f;

                // Basement should have significant light from BasementOffice (at least 1.0)
                Assert.True(basementLight > 1.0f, 
                    $"Expected Basement light level > 1.0, got {basementLight:F3}");

                // Basement should have LESS light than BasementOffice (decay should occur)
                Assert.True(basementLight < officeLight, 
                    $"Expected Basement light ({basementLight:F2}) < BasementOffice light ({officeLight:F2})");

                return TestResult.PASS;
            }

            return TestResult.CONTINUE;
        });

        game.Run();
    }

    [Fact]
    public void LightDecreasesAcrossConnectedRoomChain()
    {
        // Arrange
        var basement = new HomeWorldSegmentGroup();

        // Act - Test light propagation through: BasementOffice -> Basement -> UpstairsHall -> Kitchen -> LivingRoom
        bool lightsConfigured = false;
        Room[] roomChain = null;

        using var game = new TestGame(basement, framesToRun: 100, testAssertion: (g, gameTime) =>
        {
            // On first update, configure lights
            if (gameTime.TotalGameTime.TotalMilliseconds < 50)
            {
                var loadedLevelData = g.GetService<LoadedLevelData>();

                g.SetAllLights(l => l.Room is BasementOffice);

                // Find rooms in the chain by type
                var allRooms = loadedLevelData.LoadedSegments
                    .SelectMany(ld => ld.WorldSegment.TraverseAllChildren())
                    .OfType<Room>()
                    .ToList();

                roomChain = new Room[]
                {
                    allRooms.OfType<BasementOffice>().First(),
                    allRooms.OfType<Basement>().First(),
                    allRooms.OfType<BasementStairs>().First()
                };

                lightsConfigured = true;
                return TestResult.CONTINUE;
            }

            // After lights are configured, check the light propagation
            if (lightsConfigured && gameTime.TotalGameTime.TotalMilliseconds > 200)
            {
                var loadedLevelData = g.GetService<LoadedLevelData>();
                var lightCalc = loadedLevelData.LightingCalculator;

                // Get light levels for each room in the chain
                var lightLevels = roomChain.Select(room =>
                {
                    lightCalc.RoomLightGraph.TryGet(room, out var lightData);
                    return lightData?.GetTotalLight() ?? 0f;
                }).ToArray();

                // Assert: Each room should have less light than the previous room
                for (int i = 0; i < lightLevels.Length - 1; i++)
                {
                    var currentRoom = roomChain[i].GetType().Name;
                    var nextRoom = roomChain[i + 1].GetType().Name;
                    var currentLight = lightLevels[i];
                    var nextLight = lightLevels[i + 1];

                    Assert.True(currentLight >= nextLight,
                        $"{currentRoom} ({currentLight:F2}) should be >= {nextRoom} ({nextLight:F2})");


                    Assert.True(currentLight > 0,
                        $"{currentRoom} ({currentLight:F2}) should be > 0");
                }

                // BasementOffice should have light
                Assert.True(lightLevels[0] > 0.0f, $"BasementOffice should have light, got {lightLevels[0]}");

                return TestResult.PASS;
            }

            return TestResult.CONTINUE;
        });

        game.Run();
    }

    // retiring this test as it will change too often
    //[Fact]
    //public void RoomGraphHasExpectedStructure()
    //{
    //    // Arrange
    //    var basement = new BasementWorldSegment();

    //    using var game = new TestGame(basement, framesToRun: 10, testAssertion: (g, gameTime) =>
    //    {
    //        if (gameTime.TotalGameTime.TotalMilliseconds > 50)
    //        {
    //            var loadedLevelData = g.GetService<LoadedLevelData>();
    //            var basementOffice = basement.TraverseAllChildren().OfType<BasementOffice>().First();

    //            var treeView = BuildRoomGraphTree(basementOffice, loadedLevelData.RoomGraph);

    //            var expected = File.ReadAllText("Fixtures\\ExpectedRoomGraph.txt").Trim();
    //            File.WriteAllText("Fixtures\\ActualRoomGraph.txt", treeView);
    //            Assert.Equal(expected.Trim(), treeView.Trim());
    //            return TestResult.PASS;
    //        }

    //        return TestResult.CONTINUE;
    //    });

    //    game.Run();
    //}

    private string BuildRoomGraphTree(IRoom startRoom, RoomGraph roomGraph, int indent = 0, HashSet<IRoom> visited = null)
    {
        visited ??= new HashSet<IRoom>();

        if (visited.Contains(startRoom))
            return "";

        visited.Add(startRoom);

        var indentation = new string(' ', indent * 2);
        var lightingGroupName = startRoom?.ToString() ?? "null";
        var result = $"{indentation}{startRoom} (LightingGroup: {lightingGroupName})\n";

        var neighbors = roomGraph.GetNeighbors(startRoom);
        foreach (var neighbor in neighbors)
        {
            result += BuildRoomGraphTree(neighbor, roomGraph, indent + 1, visited);
        }

        return result;
    }
}
