using ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;
using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments;

/// <summary>
/// unreachable segment with basic shapes simulating other houses
/// </summary>
public class NeighborhoodWorldSegment : WorldSegment 
{
    public NeighborhoodWorldSegment()
    {
        var ground = AddChild(new Box(new Theme(TextureSheetKey.Outdoors, TextureKey.Grass, Color.White)));
        
        var roadPlaceholder = new PlaceholderShape<Road>(this, tag: "HomeRoad",
            position: new Vector3(-29.779997f, 6.7200003f, -9.839999f),
            size: new Vector3(9.599999f, 7.68f, 65.36f));

        ground.AdjustShape().From(roadPlaceholder)
            .AxisStretch(Axis.X, Measure.Feet(100))
            .AxisStretch(Axis.Z, Measure.Feet(100));

        ground.Height = 1.0f;

        ground.Place().OnFloor(roadPlaceholder)
                      .OnSideOuter(Side.West, roadPlaceholder);
        ground.Y -= ground.Height;


        var fakeHouse = new FakeHouse(this, ground);

        fakeHouse.X += Measure.Feet(5);
    }
}
