using ExploringGame.Tests.TestHelpers;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Xunit;

namespace ExploringGame.Tests.WallDecalPlacement;

public class WallDecalPlacementTests
{
    [Fact]
    public void WallDecals_DoNotAppearOverGap()
    {
        var worldSegment = new WallWithGapWorldSegment();

        using var game = new TestGame(worldSegment, TimeSpan.FromSeconds(60));        
        game.Run();

        // assertion handled within test controller
    }
}
