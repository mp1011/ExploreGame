using ExploringGame.Testing;
using ExploringGame.Tests.TestHelpers;
using Xunit;

namespace ExploringGame.Tests;

public class WallDecalTests
{
    [Fact]
    public void WallDecalTest_LoadsAndRenders_MatchesReferenceImage()
    {
        // Arrange
        var worldSegment = TestMaps.WallDecalTest();
        
        using var game = new TestGame(worldSegment, TimeSpan.FromSeconds(5));
        
        // Act
        game.Run();

        // Assert - Compare screenshot to reference image
        game.AssertScreenshot("Fixtures/WallDecalTest.png", maxAverageDifference: 5.0);
    }
}
