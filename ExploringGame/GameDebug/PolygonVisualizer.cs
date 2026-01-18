using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Color = System.Drawing.Color;

namespace ExploringGame.GameDebug;

public class PolygonVisualizer
{
    private const int Width = 500;
    private const int Height = 500;

    private static int FileIndex = 1;
    public static void SavePolygonImage(string filename, params IPolygon2D[] polygons)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            throw new Exception("not available");

        using var image = CreatePolygonImage(polygons.ToList());        
        image?.Save($"{FileIndex.ToString("000")}_{filename}.bmp");

        FileIndex++;
    }

    /// <summary>
    /// Creates an image of a convex polygon given its vertices.
    /// </summary>
    /// <param name="vertices">List of (x, y) points representing the polygon.</param>
    /// <param name="width">Width of the resulting image in pixels.</param>
    /// <param name="height">Height of the resulting image in pixels.</param>
    /// <param name="fillColor">Color to fill the polygon.</param>
    /// <param name="borderColor">Color of the polygon border.</param>
    /// <returns>A Bitmap containing the drawn polygon.</returns>
    public static Bitmap CreatePolygonImage(List<IPolygon2D> polygons)
    {
        if (!polygons.Any())
            return null;

        if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            throw new Exception("not available");
        
        Color[] fillColors = new Color[] { Color.Pink, Color.LightGreen, Color.LightBlue, Color.Cyan, Color.Yellow, Color.Magenta };
        int index = 0;
        Color borderColor = Color.Black;

        var minX = polygons.SelectMany(p => p.Vertices).Min(p => p.X);
        var minY = polygons.SelectMany(p => p.Vertices).Min(p => p.Y);

        var translate = new Vector2(minX < 0 ? -minX : 0, minY < 0 ? -minY : 0);
        var scale = new Vector2(50f, 50f);

        Bitmap bmp = new Bitmap(Width, Height);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.White);

            foreach (var polygon in polygons)
            {
                var fillColor = fillColors[index % fillColors.Length];
                var vertices = polygon.Vertices.Select(p=> (p + translate) * scale).ToArray();


                using (Brush brush = new SolidBrush(fillColor))
                using (Pen pen = new Pen(borderColor, 2))
                {
                    g.FillPolygon(brush, vertices.Select(p=> new PointF(p.X,p.Y)).ToArray());
                    g.DrawPolygon(pen, vertices.Select(p => new PointF(p.X, p.Y)).ToArray());
                }

                index++;
            }
        }

        return bmp;
    }
}