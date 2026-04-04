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
    public NeighborhoodWorldSegment()
    {
        var roadPlaceholder = new PlaceholderShape<Road>(this, tag: "HomeRoad",
            position: new Vector3(-29.779997f, 6.7200003f, -9.839999f),
            size: new Vector3(9.599999f, 7.68f, 65.36f));

        var westNeighborhoodBlock = new BgNeighborhood(this);
        westNeighborhoodBlock.Ground.Place()
            .OnFloor(roadPlaceholder)
            .OnSideOuter(Side.West, roadPlaceholder);
        westNeighborhoodBlock.Ground.Y -= westNeighborhoodBlock.Ground.Height;


        var northNeighborhoodBlock = new BgNeighborhood(this);
        northNeighborhoodBlock.Ground.Place()
          .OnFloor(roadPlaceholder)
          .OnSideOuter(Side.North, roadPlaceholder)
          .OnSideOuter(Side.East, westNeighborhoodBlock.Ground);
        northNeighborhoodBlock.Ground.Y -= northNeighborhoodBlock.Height;
        northNeighborhoodBlock.Ground.Z -= 20f;


        var southNeighborhoodBlock = new BgNeighborhood(this);
        southNeighborhoodBlock.Ground.Place()
          .OnFloor(roadPlaceholder)
          .OnSideOuter(Side.South, roadPlaceholder)
          .OnSideOuter(Side.East, westNeighborhoodBlock.Ground);
        southNeighborhoodBlock.Ground.Y -= southNeighborhoodBlock.Height;
        southNeighborhoodBlock.Ground.Z += 20f;

        westNeighborhoodBlock.LoadChildren();
        northNeighborhoodBlock.LoadChildren();
        southNeighborhoodBlock.LoadChildren();
    }
}
