using ExploringGame.GeometryBuilder;
using System;
using System.IO;

namespace ExploringGame.GameDebug;

public static class DebugShapeLogger
{
    private static readonly string _logFilePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        $"ShapeLog_{DateTime.Now:yyyy-MM-dd}.txt");

    public static void LogShape(string label, Shape shape)
    {
        var logEntry = $"[{DateTime.Now:HH:mm:ss.fff}] {label} - " +
                      $"Type: {shape.GetType().Name}, " +
                      $"Tag: {shape.Tag ?? "null"}, " +
                      $"Position: ({shape.Position.X:F2}, {shape.Position.Y:F2}, {shape.Position.Z:F2}), " +
                      $"Size: ({shape.Size.X:F2}, {shape.Size.Y:F2}, {shape.Size.Z:F2})";

        File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
    }
}
