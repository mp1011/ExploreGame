using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;

public class BgNeighborhood : Room
{
    public override ViewFrom ViewFrom => ViewFrom.None;

    public Shape Ground { get; }

    public BgNeighborhood(WorldSegment worldSegment) : base(worldSegment)
    {
        FixedAmbientLight = LightIntensity.Bright;

        Ground = AddChild(new Box(new Theme(TextureSheetKey.Outdoors, TextureKey.Grass, Color.White)));

        Ground.Height = Measure.Feet(20);
        Ground.AdjustShape()
            .AxisStretch(Axis.X, Measure.Feet(900))
            .AxisStretch(Axis.Z, Measure.Feet(900));

        Ground.Height = 1.0f;       
    }

    public void LoadChildren()
    {
        CreateHouseGrid();
    }

    private void CreateHouseGrid()
    {
        var gridCountX = 10;
        var gridCountZ = 10;

        // Calculate spacing to fit houses within ground bounds with some margin
        var marginX = Measure.Feet(50);
        var marginZ = Measure.Feet(50);
        var availableX = Ground.Width - marginX * 2;
        var availableZ = Ground.Depth - marginZ * 2;

        var spacingX = availableX / (gridCountX - 1);
        var spacingZ = availableZ / (gridCountZ - 1);

        // Start position: ground center minus half its size, plus margin
        var startX = Ground.X - (Ground.Width / 2) + marginX;
        var startZ = Ground.Z - (Ground.Depth / 2) + marginZ;

        for (int x = 0; x < gridCountX; x++)
        {
            for (int z = 0; z < gridCountZ; z++)
            {
                var fakeHouse = new FakeHouse(this, Ground);

                fakeHouse.X = startX + (x * spacingX);
                fakeHouse.Z = startZ + (z * spacingZ);
            }
        }
    }
}
