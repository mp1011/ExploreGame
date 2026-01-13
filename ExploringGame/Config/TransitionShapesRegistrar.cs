using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ExploringGame.Config;

public record TransitionShapeInfo(float X, float Y, float Z, float Width, float Height, float Depth)
{
    public static TransitionShapeInfo Create(Vector3 Position, Vector3 Size) =>
        new TransitionShapeInfo(Position.X, Position.Y, Position.Z, Size.X, Size.Y, Size.Z);
}

public class TransitionShapesRegistrar
{
    private const string JsonFilePath = "Config/transition_shapes.json";

    private Dictionary<string, TransitionShapeInfo> _transitionShapes;
        
    public TransitionShapesRegistrar()
    {
        _transitionShapes = LoadFromDisk();
    }

    public TransitionShapeInfo Get<T>() where T : Shape
    {
        TransitionShapeInfo info;
        if(!_transitionShapes.TryGetValue(Key<T>(), out info))
            throw new System.Exception($"No info found for {Key<T>()}");

        return info;
    }

    public void Set<T>(T shape) where T : Shape
    {
        if(_transitionShapes.ContainsKey(Key<T>()))
        {
            var existing = Get<T>();
            if (existing.X == shape.X && existing.Y == shape.Y && existing.Z == shape.Z
                && existing.Width == shape.Width && existing.Depth == shape.Depth && existing.Height == shape.Height)
                return;
        }

        _transitionShapes[Key<T>()] = TransitionShapeInfo.Create(shape.Position, shape.Size);
        SaveToDisk();
    }

    public void RecallPositionAndSize<T>(T shape) where T:Shape
    {
        var info = Get<T>();
        shape.Position = new Vector3(info.X, info.Y, info.Z);
        shape.Size = new Vector3(info.Width, info.Height, info.Depth);
    }

    private string Key<T>()
    {
        return typeof(T).Name;
    }

    private Dictionary<string, TransitionShapeInfo> LoadFromDisk()
    {
        if (!File.Exists(JsonFilePath))
            return new Dictionary<string, TransitionShapeInfo>();

        var json = File.ReadAllText(JsonFilePath);
        return JsonSerializer.Deserialize<Dictionary<string, TransitionShapeInfo>>(json)
               ?? new Dictionary<string, TransitionShapeInfo>();
    }

    private void SaveToDisk()
    {
        var dir = Path.GetDirectoryName(JsonFilePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(_transitionShapes, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(JsonFilePath, json);
    }
}
