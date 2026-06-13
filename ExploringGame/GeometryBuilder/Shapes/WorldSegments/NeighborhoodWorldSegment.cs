using ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;
using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments;

/// <summary>
/// unreachable segment with basic shapes simulating other houses
/// </summary>
public class NeighborhoodWorldSegment : WorldSegment 
{
    private BgNeighborhood _westNeighborhoodBlock;
    private BgNeighborhood _northNeighborhoodBlock;
    private BgNeighborhood _southNeighborhoodBlock;

    public NeighborhoodWorldSegment()
    {
        SetLocalSide(Side.Bottom, UpstairsWorldSegment.FloorY - Measure.Feet(4));
        _westNeighborhoodBlock = new BgNeighborhood(this);
        _northNeighborhoodBlock = new BgNeighborhood(this);
        _southNeighborhoodBlock = new BgNeighborhood(this);
    }

    public override void PositionChildren(IEnumerable<WorldSegment> loadedSegments)
    {
        // Find the road from OutsideWorldSegment
        var homeRoad = FindShapeByTag<Road>(loadedSegments, "HomeRoad");

        // Position neighborhood blocks relative to the road
        _westNeighborhoodBlock.Ground.Place()
            .OnFloor(homeRoad)
            .OnSideOuter(Side.West, homeRoad);
        _westNeighborhoodBlock.Ground.LocalY -= _westNeighborhoodBlock.Ground.Height;

        _northNeighborhoodBlock.Ground.Place()
          .OnFloor(homeRoad)
          .OnSideOuter(Side.North, homeRoad)
          .OnSideOuter(Side.East, _westNeighborhoodBlock.Ground);
        _northNeighborhoodBlock.Ground.LocalY -= _northNeighborhoodBlock.Height;
        _northNeighborhoodBlock.Ground.LocalZ -= 20f;

        _southNeighborhoodBlock.Ground.Place()
          .OnFloor(homeRoad)
          .OnSideOuter(Side.South, homeRoad)
          .OnSideOuter(Side.East, _westNeighborhoodBlock.Ground);
        _southNeighborhoodBlock.Ground.LocalY -= _southNeighborhoodBlock.Height;
        _southNeighborhoodBlock.Ground.LocalZ += 20f;

        // Load children after all positioning is complete
        _westNeighborhoodBlock.LoadChildren();
        _northNeighborhoodBlock.LoadChildren();
        _southNeighborhoodBlock.LoadChildren();
    }
}
