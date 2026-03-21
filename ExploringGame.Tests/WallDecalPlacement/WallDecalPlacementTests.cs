using ExploringGame.GeometryBuilder;
using ExploringGame.Logics;
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

    [Fact]
    public void WallDecals_OnlyAppearOnWestSide_WhenEastQuadTooSmall()
    {
        var worldSegment = new WallWithAsymmetricGapWorldSegment(Side.North);

        using var game = new TestGame(worldSegment, TimeSpan.FromSeconds(60));
        game.Run();

        // Get the test controller through the world segment's test shape
        var testController = worldSegment.TestShape.Controller;
        var placedDecals = testController.PlacedDecals;

        // All decals should be on the west side of the gap
        // The east side quad should be too small to fit any decals
        Assert.True(placedDecals.Any(), "Expected at least one decal to be placed");
        
        foreach (var decal in placedDecals)
        {
            var decalX = decal.Position.X;
            Assert.True(decalX < worldSegment.GapStartX, 
                $"Decal at X={decalX:F2} should be west of gap (gap starts at {worldSegment.GapStartX:F2})");
        }
    }

    [Fact]
    public void WallDecals_OnlyAppearOnEastSide_WhenWestQuadTooSmall_SouthWall()
    {
        var worldSegment = new WallWithAsymmetricGapWorldSegment(Side.South);

        using var game = new TestGame(worldSegment, TimeSpan.FromSeconds(60));
        game.Run();

        var testController = worldSegment.TestShape.Controller;
        var placedDecals = testController.PlacedDecals;

        Assert.True(placedDecals.Any(), "Expected at least one decal to be placed");

        // For South wall, HAlign.Right with position inversion places gap on West side
        // So decals should appear on the East side (larger quad)
        foreach (var decal in placedDecals)
        {
            var decalX = decal.Position.X;
            Assert.True(decalX > worldSegment.GapEndX, 
                $"Decal at X={decalX:F2} should be east of gap (gap ends at {worldSegment.GapEndX:F2})");
        }
    }

    [Fact]
    public void WallDecals_OnlyAppearOnNorthSide_WhenSouthQuadTooSmall_EastWall()
    {
        var worldSegment = new WallWithAsymmetricGapWorldSegment(Side.East);

        using var game = new TestGame(worldSegment, TimeSpan.FromSeconds(60));
        game.Run();

        var testController = worldSegment.TestShape.Controller;
        var placedDecals = testController.PlacedDecals;

        Assert.True(placedDecals.Any(), "Expected at least one decal to be placed");
        
        foreach (var decal in placedDecals)
        {
            var decalZ = decal.Position.Z;
            Assert.True(decalZ < worldSegment.GapStartX, 
                $"Decal at Z={decalZ:F2} should be north of gap (gap starts at {worldSegment.GapStartX:F2})");
        }
    }

    [Fact]
    public void WallDecals_OnlyAppearOnSouthSide_WhenNorthQuadTooSmall_WestWall()
    {
        var worldSegment = new WallWithAsymmetricGapWorldSegment(Side.West);

        using var game = new TestGame(worldSegment, TimeSpan.FromSeconds(60));
        game.Run();

        var testController = worldSegment.TestShape.Controller;
        var placedDecals = testController.PlacedDecals;

        Assert.True(placedDecals.Any(), "Expected at least one decal to be placed");

        // For West wall, HAlign.Right with position inversion places gap on North side
        // So decals should appear on the South side (larger quad)
        foreach (var decal in placedDecals)
        {
            var decalZ = decal.Position.Z;
            Assert.True(decalZ > worldSegment.GapEndX, 
                $"Decal at Z={decalZ:F2} should be south of gap (gap ends at {worldSegment.GapEndX:F2})");
        }
    }
}

