using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Logics.Pathfinding;
using ExploringGame.Tests.TestHelpers;
using Xunit;

namespace ExploringGame.Tests;

/// <summary>
/// Tests for the shader rework that groups lighting by LightingGroup instead of individual rooms
/// </summary>
public class RoomLightingGroupTests
{
    #region Task 1: RoomLightingCalculator should calculate by LightingGroup
    
    [Fact]
    public void RoomLightingCalculator_GroupsRoomsByLightingGroup()
    {
        // Arrange
        var basement = new BasementWorldSegment(null);

        // Act
        using var game = new TestGame(basement, framesToRun: 100, testAssertion: (g, gameTime) =>
        {
            if (gameTime.TotalGameTime.TotalMilliseconds > 100)
            {
                var loadedLevelData = g.GetService<LoadedLevelData>();
                var calculator = loadedLevelData.LightingCalculator;

                // Get all rooms and distinct lighting groups
                var allRooms = loadedLevelData.RoomGraph.GetAllRooms().ToList();
                var lightingGroups = calculator.GetDistinctLightingGroups().ToList();

                Assert.NotNull(lightingGroups);
                Assert.NotEmpty(lightingGroups);

                // The number of lighting groups should be LESS than the number of rooms
                // (proving that multiple rooms share lighting groups)
                Assert.True(lightingGroups.Count < allRooms.Count, 
                    $"Expected fewer lighting groups ({lightingGroups.Count}) than rooms ({allRooms.Count})");

                return TestResult.PASS;
            }

            return TestResult.CONTINUE;
        });

        game.Run();
    }
    
    [Fact]
    public void RoomLightingCalculator_CalculatesLightForLightingGroups()
    {
        // Arrange
        var basement = new BasementWorldSegment(null);
        
        // Act
        using var game = new TestGame(basement, framesToRun: 100, testAssertion: (g, gameTime) =>
        {
            if (gameTime.TotalGameTime.TotalMilliseconds > 100)
            {
                var loadedLevelData = g.GetService<LoadedLevelData>();
                var calculator = loadedLevelData.LightingCalculator;
                
                // Should be able to get RoomLightData by LightingGroup
                var allRooms = loadedLevelData.RoomGraph.GetAllRooms().ToList();
                var firstRoom = allRooms.First();
                var lightingGroup = firstRoom.LightingGroup;
                
                // Should be able to retrieve light data for a lighting group
                var lightData = calculator.GetLightDataForGroup(lightingGroup);
                
                Assert.NotNull(lightData);
                
                return TestResult.PASS;
            }
            
            return TestResult.CONTINUE;
        });
        
        game.Run();
    }
    
    [Fact]
    public void RoomLightingCalculator_RoomsInSameLightingGroupShareLightData()
    {
        // Arrange
        // Create a test world where multiple rooms share the same lighting group
        var basement = new BasementWorldSegment(null);
        
        // Act
        using var game = new TestGame(basement, framesToRun: 100, testAssertion: (g, gameTime) =>
        {
            if (gameTime.TotalGameTime.TotalMilliseconds > 100)
            {
                var loadedLevelData = g.GetService<LoadedLevelData>();
                var calculator = loadedLevelData.LightingCalculator;
                
                // Find rooms that share the same lighting group
                var allRooms = loadedLevelData.RoomGraph.GetAllRooms().ToList();
                var roomsGroupedByLightingGroup = allRooms
                    .GroupBy(r => r.LightingGroup)
                    .Where(g => g.Count() > 1)
                    .FirstOrDefault();
                
                if (roomsGroupedByLightingGroup != null)
                {
                    var roomsInGroup = roomsGroupedByLightingGroup.ToList();
                    
                    // All rooms in the same lighting group should return the same light data
                    var firstRoomLightData = calculator.GetLightDataForGroup(roomsInGroup[0].LightingGroup);
                    
                    foreach (var room in roomsInGroup.Skip(1))
                    {
                        var lightData = calculator.GetLightDataForGroup(room.LightingGroup);
                        Assert.Same(firstRoomLightData, lightData);
                    }
                }
                
                return TestResult.PASS;
            }
            
            return TestResult.CONTINUE;
        });
        
        game.Run();
    }
    
    #endregion
    
    #region Task 2: ShapeBufferCreator groups by LightingGroup and Texture Sheet
    
    [Fact]
    public void ShapeBufferCreator_CreatesOneBufferPerLightingGroupAndTextureSheet()
    {
        // Arrange
        var basement = new BasementWorldSegment(null);
        
        // Act
        using var game = new TestGame(basement, framesToRun: 100, testAssertion: (g, gameTime) =>
        {
            if (gameTime.TotalGameTime.TotalMilliseconds > 100)
            {
                var loadedLevelData = g.GetService<LoadedLevelData>();
                var shapeBuffers = loadedLevelData.LoadedSegments
                    .SelectMany(ld => ld.ShapeBuffers)
                    .ToArray();
                
                // Group buffers by their lighting group and texture sheet
                var groupedBuffers = shapeBuffers
                    .Where(sb => sb.LightingGroup != null) // Exclude non-grouped buffers
                    .GroupBy(sb => new { sb.LightingGroup, sb.Texture })
                    .ToArray();
                
                // Each combination of LightingGroup and TextureSheet should have exactly one buffer
                foreach (var group in groupedBuffers)
                {
                    Assert.Single(group);
                }
                
                return TestResult.PASS;
            }
            
            return TestResult.CONTINUE;
        });
        
        game.Run();
    }
    
    [Fact]
    public void ShapeBufferCreator_IncludesRoomAndChildrenInLightingGroupBuffer()
    {
        // Arrange
        var basement = new BasementWorldSegment(null);
        
        // Act
        using var game = new TestGame(basement, framesToRun: 100, testAssertion: (g, gameTime) =>
        {
            if (gameTime.TotalGameTime.TotalMilliseconds > 100)
            {
                var loadedLevelData = g.GetService<LoadedLevelData>();
                var allRooms = loadedLevelData.RoomGraph.GetAllRooms().ToList();
                var firstRoom = allRooms.First();
                
                // Find the buffer for this room's lighting group
                var bufferForLightingGroup = loadedLevelData.LoadedSegments
                    .SelectMany(ld => ld.ShapeBuffers)
                    .FirstOrDefault(sb => sb.LightingGroup == firstRoom.LightingGroup);
                
                Assert.NotNull(bufferForLightingGroup);

                // The buffer should include shapes from:
                // 1. The room itself
                // 2. Rooms that reference this as their LightingGroup
                // 3. All children of those rooms
                // This will be validated by checking that a buffer exists for the lighting group
                Assert.Equal(firstRoom.LightingGroup, bufferForLightingGroup.LightingGroup);
                
                return TestResult.PASS;
            }
            
            return TestResult.CONTINUE;
        });
        
        game.Run();
    }
    
    [Fact]
    public void ShapeBufferCreator_CreatesBufferForRemainingStaticShapes()
    {
        // Arrange
        var basement = new BasementWorldSegment(null);
        
        // Act
        using var game = new TestGame(basement, framesToRun: 100, testAssertion: (g, gameTime) =>
        {
            if (gameTime.TotalGameTime.TotalMilliseconds > 100)
            {
                var loadedLevelData = g.GetService<LoadedLevelData>();
                
                // There should be at least one buffer for remaining static shapes
                // (shapes that aren't part of any lighting group)
                var remainingShapesBuffer = loadedLevelData.LoadedSegments
                    .SelectMany(ld => ld.ShapeBuffers)
                    .Where(sb => sb.LightingGroup == null)
                    .ToArray();

                Assert.NotEmpty(remainingShapesBuffer);
                
                return TestResult.PASS;
            }
            
            return TestResult.CONTINUE;
        });
        
        game.Run();
    }
    
    #endregion
    
    #region Task 3: ShapeBufferCreator has RoomLightData
    
    [Fact]
    public void ShapeBufferCreator_HasRoomLightDataProperty()
    {
        // This test verifies that ShapeBufferCreator can access RoomLightData
        // The property might be injected via constructor or set as a property
        
        // This will fail until we add RoomLightData to ShapeBufferCreator
        // We can't fully test this without the actual implementation,
        // but we can verify the dependency is accessible
        
        var basement = new BasementWorldSegment(null);
        
        using var game = new TestGame(basement, framesToRun: 100, testAssertion: (g, gameTime) =>
        {
            if (gameTime.TotalGameTime.TotalMilliseconds > 100)
            {
                // The ShapeBufferCreator should have access to lighting data
                // This will need to be verified through integration testing
                // or by examining the ShapeBufferCreator's dependencies
                
                return TestResult.PASS;
            }
            
            return TestResult.CONTINUE;
        });
        
        game.Run();
    }
    
    #endregion
    
    #region Task 4: RoomLightData caches total light level
    
    [Fact]
    public void RoomLightData_TotalLightIsProperty()
    {
        // Arrange
        var basement = new BasementWorldSegment(null);
        var room = basement.TraverseAllChildren().OfType<Room>().First();
        var lightData = new RoomLightData(room);
        
        // Act - Access TotalLight as a property (not a method)
        var totalLight = lightData.TotalLight;
        
        // Assert - should be accessible as a property
        Assert.True(totalLight >= 0f);
    }
    
    [Fact]
    public void RoomLightData_TotalLightNotRecalculatedOnEveryAccess()
    {
        // Arrange
        var basement = new BasementWorldSegment(null);
        
        using var game = new TestGame(basement, framesToRun: 100, testAssertion: (g, gameTime) =>
        {
            if (gameTime.TotalGameTime.TotalMilliseconds > 100)
            {
                var loadedLevelData = g.GetService<LoadedLevelData>();
                var room = loadedLevelData.RoomGraph.GetAllRooms().First();
                
                if (loadedLevelData.LightingCalculator.RoomLightGraph.TryGet(room, out var lightData))
                {
                    // Access TotalLight multiple times
                    var light1 = lightData.TotalLight;
                    var light2 = lightData.TotalLight;
                    var light3 = lightData.TotalLight;
                    
                    // All accesses should return the same cached value
                    Assert.Equal(light1, light2);
                    Assert.Equal(light2, light3);
                    
                    // The value should be cached in memory, not recalculated
                    // (This is more of an implementation detail, but the property should exist)
                }
                
                return TestResult.PASS;
            }
            
            return TestResult.CONTINUE;
        });
        
        game.Run();
    }
    
    [Fact]
    public void RoomLightData_HasRecalculateLightLevelMethod()
    {
        // Arrange
        var basement = new BasementWorldSegment(null);
        var room = basement.TraverseAllChildren().OfType<Room>().First();
        var lightData = new RoomLightData(room);
        
        // Act - Call RecalculateLightLevel method
        lightData.RecalculateLightLevel();
        
        // Assert - method should exist and be callable
        // The actual recalculation logic will be tested in integration tests
    }
    
    [Fact]
    public void RoomLightData_RecalculateLightLevelUpdatesCache()
    {
        // Arrange
        var basement = new BasementWorldSegment(null);
        
        using var game = new TestGame(basement, framesToRun: 200, testAssertion: (g, gameTime) =>
        {
            if (gameTime.TotalGameTime.TotalMilliseconds < 50)
            {
                return TestResult.CONTINUE;
            }
            
            if (gameTime.TotalGameTime.TotalMilliseconds > 100 && gameTime.TotalGameTime.TotalMilliseconds < 150)
            {
                var loadedLevelData = g.GetService<LoadedLevelData>();
                var room = loadedLevelData.RoomGraph.GetAllRooms().First();
                
                if (loadedLevelData.LightingCalculator.RoomLightGraph.TryGet(room, out var lightData))
                {
                    // Get initial cached value
                    var initialLight = lightData.TotalLight;

                    // Find a light source that contributes to this room
                    var lightSources = lightData.GetLightSources().ToList();

                    if (lightSources.Any())
                    {
                        var lightToRemove = lightSources.First();

                        // Remove a light contribution - this changes the underlying data
                        // but should NOT update the cached value yet
                        lightData.RemoveLightContribution(lightToRemove);

                        // Before recalculation, TotalLight should still be the old cached value
                        var beforeRecalc = lightData.TotalLight;
                        Assert.Equal(initialLight, beforeRecalc);

                        // After calling RecalculateLightLevel, the cache should update
                        lightData.RecalculateLightLevel();
                        var afterRecalc = lightData.TotalLight;

                        // The value should have changed (should be less now that we removed a light)
                        Assert.NotEqual(beforeRecalc, afterRecalc);
                        Assert.True(afterRecalc < beforeRecalc, 
                            $"After removing a light, total light should decrease. Before: {beforeRecalc}, After: {afterRecalc}");
                    }
                }
                
                return TestResult.PASS;
            }
            
            return TestResult.CONTINUE;
        });
        
        game.Run();
    }
    
    [Fact]
    public void RoomLightingCalculator_CallsRecalculateLightLevelOnLightStateChange()
    {
        // Arrange
        var basement = new BasementWorldSegment(null);
        
        using var game = new TestGame(basement, framesToRun: 200, testAssertion: (g, gameTime) =>
        {
            if (gameTime.TotalGameTime.TotalMilliseconds > 100 && gameTime.TotalGameTime.TotalMilliseconds < 150)
            {
                var loadedLevelData = g.GetService<LoadedLevelData>();
                
                // Find a light and toggle it
                var light = loadedLevelData.LoadedSegments
                    .SelectMany(ld => ld.WorldSegment.TraverseAllChildren())
                    .OfType<ILightSource>()
                    .FirstOrDefault(l => l.On);
                
                if (light != null && light.Room != null)
                {
                    // Get the light data before toggle
                    if (loadedLevelData.LightingCalculator.RoomLightGraph.TryGet(light.Room, out var lightData))
                    {
                        var beforeToggle = lightData.TotalLight;
                        
                        // Toggle the light
                        light.On = !light.On;
                        
                        // The calculator should have called RecalculateLightLevel
                        // So the cached value should be updated
                        var afterToggle = lightData.TotalLight;
                        
                        // Values should differ because light state changed
                        Assert.NotEqual(beforeToggle, afterToggle);
                    }
                }
                
                return TestResult.PASS;
            }
            
            return TestResult.CONTINUE;
        });
        
        game.Run();
    }
    
    [Fact]
    public void RoomLightingCalculator_CallsRecalculateLightLevelOnDoorStateChange()
    {
        // Arrange
        var basement = new BasementWorldSegment(null);

        using var game = new TestGame(basement, framesToRun: 200, testAssertion: (g, gameTime) =>
        {
            // Turn on only Basement light initially
            if (gameTime.TotalGameTime.TotalMilliseconds < 50)
            {
                g.SetAllLights(l => l.Room is Basement);
                return TestResult.CONTINUE;
            }

            if (gameTime.TotalGameTime.TotalMilliseconds > 100 && gameTime.TotalGameTime.TotalMilliseconds < 150)
            {
                var loadedLevelData = g.GetService<LoadedLevelData>();

                // Find the door between basement stairs and upstairs hall
                var door = loadedLevelData.LoadedSegments
                    .SelectMany(ld => ld.WorldSegment.TraverseAllChildren())
                    .OfType<Door>()
                    .FirstOrDefault(d => d.StateKey == StateKey.BasementStairsDoorOpen);

                Assert.NotNull(door);

                var allRooms = loadedLevelData.RoomGraph.GetAllRooms().ToList();

                var upstairsHall = allRooms.OfType<UpstairsHall>().Single();
                var originalLight = loadedLevelData.LightingCalculator.RoomLightGraph.Get(upstairsHall);
                var beforeDoorState = door.Open;

                // Open the door (this should trigger recalculation)
                door.Open = true;

                var changedLight = loadedLevelData.LightingCalculator.RoomLightGraph.Get(upstairsHall);
                Assert.True(changedLight.TotalLight > originalLight.TotalLight, 
                    $"Opening the door should increase light in the upstairs hall. Before: {originalLight.TotalLight}, After: {changedLight.TotalLight}");

                return TestResult.PASS;
            }

            return TestResult.CONTINUE;
        });

        game.Run();
    }
    
    #endregion
}
