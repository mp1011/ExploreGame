using ExploringGame.LevelControl;
using ExploringGame.Testing;
using ExploringGame.Tests.TestHelpers;
using Xunit;

namespace ExploringGame.Tests;

public class ShapeStampTests
{
    [Fact]
    public void ShapeStampGenerator_RunsForThreeMinutes_SpawnsExpectedNumberOfShapes()
    {
        // Arrange
        var worldSegment = TestMaps.ShapeStampTest();       
        int expectedMinimumSpawns = 500; 

        using var game = new TestGame(worldSegment, TimeSpan.FromMinutes(3));
        
        // Act
        game.Run();

        // Assert
        var loadedLevelData = game.GetService<LoadedLevelData>();
        var levelData = loadedLevelData.LoadedSegments.FirstOrDefault();
        
        Assert.NotNull(levelData);
        
        var stampedShapeCount = levelData.StampedShapeBuffers.Count;
        
        Assert.True(stampedShapeCount >= expectedMinimumSpawns, 
            $"Expected at least {expectedMinimumSpawns} stamped shapes, but found {stampedShapeCount}");
    }
}
