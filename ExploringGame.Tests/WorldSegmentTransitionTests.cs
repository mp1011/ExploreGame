using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Testing;
using ExploringGame.Tests.TestHelpers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ExploringGame.Tests;

public class WorldSegmentTransitionTests
{
    [Fact]
    public void OnlyCurrentAndNeighboringWorldSegmentsLoaded()
    {
        // Arrange: Create 5 TestWorldSegments labeled A through E in a chain
        var segmentA = new TestWorldSegmentA(new Vector3(0, 1.5f, 0));
        segmentA.Depth = 10f;
        segmentA.Width = 10f;
        segmentA.Height = 10f;
        segmentA.SetSide(Side.North, 0f);

        var segmentB = new TestWorldSegmentB(new Vector3(0, 1.5f, -10f));
        segmentB.Depth = 10f;
        segmentB.Width = 10f;
        segmentB.Height = 10f;
        segmentB.SetSide(Side.North, -10f);

        var segmentC = new TestWorldSegmentC(new Vector3(0, 1.5f, -20f));
        segmentC.Depth = 10f;
        segmentC.Width = 10f;
        segmentC.Height = 10f;
        segmentC.SetSide(Side.North, -20f);

        var segmentD = new TestWorldSegmentD(new Vector3(0, 1.5f, -30f));
        segmentD.Depth = 10f;
        segmentD.Width = 10f;
        segmentD.Height = 10f;
        segmentD.SetSide(Side.North, -30f);

        var segmentE = new TestWorldSegmentE(new Vector3(0, 1.5f, -40f));
        segmentE.Depth = 10f;
        segmentE.Width = 10f;
        segmentE.Height = 10f;
        segmentE.SetSide(Side.North, -40f);

        // Set up transitions: A<->B<->C<->D<->E
        segmentA.AddTransition(typeof(TestWorldSegmentB));
        
        segmentB.AddTransition(typeof(TestWorldSegmentA));
        segmentB.AddTransition(typeof(TestWorldSegmentC));
        
        segmentC.AddTransition(typeof(TestWorldSegmentB));
        segmentC.AddTransition(typeof(TestWorldSegmentD));
        
        segmentD.AddTransition(typeof(TestWorldSegmentC));
        segmentD.AddTransition(typeof(TestWorldSegmentE));
        
        segmentE.AddTransition(typeof(TestWorldSegmentD));

        // Track which segments the player has visited
        var visitedSegments = new HashSet<Type>();
        var segmentTypes = new[] 
        { 
            typeof(TestWorldSegmentA), 
            typeof(TestWorldSegmentB), 
            typeof(TestWorldSegmentC), 
            typeof(TestWorldSegmentD), 
            typeof(TestWorldSegmentE) 
        };

        var testAssertion = new Func<TestGame, GameTime, TestResult>((game, gameTime) =>
        {
            var player = game.GetService<Player>();
            var loadedLevelData = game.GetService<LoadedLevelData>();

            // Determine which segment the player is currently in
            WorldSegment currentSegment = null;
            foreach (var levelData in loadedLevelData.LoadedSegments)
            {
                if (levelData.WorldSegment.ContainsPoint(player.Position))
                {
                    currentSegment = levelData.WorldSegment;
                    visitedSegments.Add(currentSegment.GetType());
                    break;
                }
            }

            if (currentSegment == null)
                return TestResult.CONTINUE;

            // Assertions based on which segment the player is in
            var loadedTypes = loadedLevelData.LoadedSegments.Select(s => s.WorldSegment.GetType()).ToHashSet();

            if (currentSegment is TestWorldSegmentA)
            {
                // In A: should have A and B loaded
                Assert.Equal(2, loadedTypes.Count);
                Assert.Contains(typeof(TestWorldSegmentA), loadedTypes);
                Assert.Contains(typeof(TestWorldSegmentB), loadedTypes);
            }
            else if (currentSegment is TestWorldSegmentB)
            {
                // In B: should have A, B, C loaded
                Assert.Equal(3, loadedTypes.Count);
                Assert.Contains(typeof(TestWorldSegmentA), loadedTypes);
                Assert.Contains(typeof(TestWorldSegmentB), loadedTypes);
                Assert.Contains(typeof(TestWorldSegmentC), loadedTypes);
            }
            else if (currentSegment is TestWorldSegmentC)
            {
                // In C: should have B, C, D loaded
                Assert.Equal(3, loadedTypes.Count);
                Assert.Contains(typeof(TestWorldSegmentB), loadedTypes);
                Assert.Contains(typeof(TestWorldSegmentC), loadedTypes);
                Assert.Contains(typeof(TestWorldSegmentD), loadedTypes);
            }
            else if (currentSegment is TestWorldSegmentD)
            {
                // In D: should have C, D, E loaded
                Assert.Equal(3, loadedTypes.Count);
                Assert.Contains(typeof(TestWorldSegmentC), loadedTypes);
                Assert.Contains(typeof(TestWorldSegmentD), loadedTypes);
                Assert.Contains(typeof(TestWorldSegmentE), loadedTypes);
            }
            else if (currentSegment is TestWorldSegmentE)
            {
                // In E: should have D and E loaded
                Assert.Equal(2, loadedTypes.Count);
                Assert.Contains(typeof(TestWorldSegmentD), loadedTypes);
                Assert.Contains(typeof(TestWorldSegmentE), loadedTypes);

                // Final assertion: player should have visited all segments
                Assert.Equal(5, visitedSegments.Count);
                foreach (var segmentType in segmentTypes)
                {
                    Assert.Contains(segmentType, visitedSegments);
                }

                return TestResult.PASS;
            }

            return TestResult.CONTINUE;
        });

        // Set up MockPlayerInput to move player forward
        var mockInput = new MockPlayerInput();
        
        // Hold forward key for 600 frames (10 seconds at 60 FPS) to walk through all segments
        for (int frame = 1; frame <= 600; frame++)
        {
            mockInput.AddKeyPress(frame, GameKey.Forward);
        }

        using var testGame = new TestGame(segmentA, 600, testAssertion);

        // Act
        testGame.Run();

        // The test will pass if we reach segment E with all assertions passing
    }
}
