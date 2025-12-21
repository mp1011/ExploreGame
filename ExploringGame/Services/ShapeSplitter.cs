using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ExploringGame.Services;

/// <summary>
/// Splits a cuboid into equally sized sections
/// </summary>
public class ShapeSplitter
{
    public (T,Vector3)[] Split<T>(T shape, int xParts, int yParts, int zParts, Func<T> factory)
        where T:Shape
    {
        var boxes = new List<(T, Vector3)>();
        var partSize = new Vector3(shape.Size.X / xParts, shape.Size.Y / yParts, shape.Size.Z / zParts);
        for (int y = 0; y < yParts; y++)
        { 
            for (int x = 0; x < xParts; x++)
            {
                for(int z = 0; z < zParts; z++)
                {
                    var boxPosition = new Vector3(
                        shape.Position.X - shape.Size.X / 2f + partSize.X / 2f + x * partSize.X,
                        shape.Position.Y - shape.Size.Y / 2f + partSize.Y / 2f + y * partSize.Y,
                        shape.Position.Z - shape.Size.Z / 2f + partSize.Z / 2f + z * partSize.Z
                    );

                    var box = factory();
                    box.Position = boxPosition;
                    box.Size = partSize;
                    box.Rotation = shape.Rotation;
                    boxes.Add((box, new Vector3(x,y,z)));
                }
            }
        }
        return boxes.ToArray();
    }
}
