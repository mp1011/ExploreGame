using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes;

class MengerSponge : Shape
{
    private readonly ShapeSplitter _shapeSplitter;

    public MengerSponge(ShapeSplitter shapeSplitter)
    {
        _shapeSplitter = shapeSplitter;
    }

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        var cubes = MengerIteration((int)(quality - 1));
        return cubes.SelectMany(p => p.BuildCuboid()).ToArray();
    }

    private MengerSponge[] MengerIteration(int steps)
    {
        var parts = MengerIteration();
        if(steps > 1)
            parts = parts.SelectMany(p => p.MengerIteration(steps - 1)).ToArray();

        return parts;
    }

    /// <summary>
    /// Divides the cube into 27 sub-cubes, and removes the middles
    /// </summary>
    /// <returns></returns>
    private MengerSponge[] MengerIteration()
    {
        var subCubes = _shapeSplitter.Split(this, 3, 3, 3, () => new MengerSponge(_shapeSplitter));
        var cubesToKeep = subCubes.Where(p =>
        {
            var pos = p.Item2;
            // Remove the center cube and the centers of each face
            if (pos.X == 1 && pos.Y == 1)
                return false;
            if (pos.Y == 1 && pos.Z == 1)
                return false;
            if (pos.X == 1 && pos.Z == 1)
                return false;
            return true;
        }).Select(p => p.Item1).ToArray();

        int i = 0;
        foreach (var cube in cubesToKeep)
        {
            cube.MainTexture = new TextureInfo(new Color((float)i++ / cubesToKeep.Length, 0, 0));
        }

        return cubesToKeep.ToArray();
    }
}
