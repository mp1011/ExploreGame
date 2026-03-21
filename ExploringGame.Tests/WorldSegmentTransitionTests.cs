using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Services;
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
    private (T segment, Room room) CreateSegmentWithRoom<T>(float northSide, Vector3? playerStart = null) 
        where T : TestWorldSegment, new()
    {
        var segment = new T();
        segment.Depth = 10f;
        segment.Width = 10f;
        segment.Height = 10f;
        segment.SetSide(Side.North, northSide);

        if (playerStart.HasValue)
            segment.PlayerStart = playerStart.Value;

        var room = new Room(segment);
        room.Depth = 10f;
        room.Width = 10f;
        room.Height = 10f;
        room.SetSide(Side.North, northSide);

        return (segment, room);
    }

    [Fact]
    public void OnlyCurrentAndNeighboringWorldSegmentsLoaded()
    {
        // Arrange: Create 5 TestWorldSegments labeled A through E in a chain
        var (segmentA, roomA) = CreateSegmentWithRoom<TestWorldSegmentA>(0f, new Vector3(0, 1.5f, 5.0f));
        var (segmentB, roomB) = CreateSegmentWithRoom<TestWorldSegmentB>(-10f);
        var (segmentC, roomC) = CreateSegmentWithRoom<TestWorldSegmentC>(-20f);
        var (segmentD, roomD) = CreateSegmentWithRoom<TestWorldSegmentD>(-30f);
        var (segmentE, roomE) = CreateSegmentWithRoom<TestWorldSegmentE>(-40f);

        // Connect the rooms together
        roomA.AddConnectingRoom(roomB, Side.North, null);
        roomB.AddConnectingRoom(roomC, Side.North, null);
        roomC.AddConnectingRoom(roomD, Side.North, null);
        roomD.AddConnectingRoom(roomE, Side.North, null);

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
            if (gameTime.TotalGameTime.TotalMilliseconds < 500)
                return TestResult.CONTINUE;

            var player = game.GetService<Player>();
            var loadedLevelData = game.GetService<LoadedLevelData>();
            var entityRoomFinder = game.GetService<EntityRoomFinder>();

            // Determine which segment the player is currently in using EntityRoomFinder
            var currentRoom = entityRoomFinder.FindRoom(player.Position);
            if (currentRoom == null)
                return TestResult.CONTINUE;

            var currentSegment = currentRoom.WorldSegment;
            if (currentSegment == null)
                return TestResult.CONTINUE;

            visitedSegments.Add(currentSegment.GetType());

            var loadedTypes = loadedLevelData.ActiveSegments.Select(s => s.WorldSegment.GetType()).ToHashSet();

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

        using var testGame = new TestGame(segmentA, TimeSpan.FromMinutes(5), testAssertion);

        testGame.MockPlayerInput.AddKeyPress(1, GameKey.Forward);

        // Act
        testGame.Run();
    }
}
