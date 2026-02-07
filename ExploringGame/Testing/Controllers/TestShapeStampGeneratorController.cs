using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.Testing;

public class TestShapeStampGeneratorController : IActiveObject
{
    private readonly LoadedLevelData _loadedLevelData;
    private float _timeSinceLastSpawn;
    private const float SpawnInterval = 0.2f; // seconds
    private const float SpawnRadius = 2.0f;
    private Random _random = new Random();

    public TestShapeStampGenerator Generator { get; set; }

    public TestShapeStampGeneratorController(LoadedLevelData loadedLevelData)
    {
        _loadedLevelData = loadedLevelData;
    }

    public void Initialize()
    {
        _timeSinceLastSpawn = 0;
    }

    public void Stop()
    {
    }

    public void Update(GameTime gameTime)
    {
        _timeSinceLastSpawn += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_timeSinceLastSpawn >= SpawnInterval)
        {
            SpawnStampedShape();
            _timeSinceLastSpawn = 0;
        }
    }

    private void SpawnStampedShape()
    {
        // Find the WorldSegment this generator belongs to
        var worldSegment = Generator.FindFirstAncestor<WorldSegment>();
        if (worldSegment == null)
            return;

        // Generate random position within radius
        var angle = _random.NextDouble() * Math.PI * 2;
        var distance = _random.NextDouble() * SpawnRadius;
        
        var offsetX = (float)(Math.Cos(angle) * distance);
        var offsetZ = (float)(Math.Sin(angle) * distance);
        var offsetY = (float)(_random.NextDouble() * 1.0); // Random Y offset 0-3 units

        var stampedShape = new TestStampedShape();
        stampedShape.Position = new Vector3(
            Generator.Position.X + offsetX,
            Generator.Position.Y + 1.0f + offsetY,
            Generator.Position.Z + offsetZ
        );

        // Add random rotation
        var randomYaw = (float)(_random.NextDouble() * Math.PI * 2); // 0 to 360 degrees
        stampedShape.Rotation = new GeometryBuilder.Rotation(randomYaw, 0, 0);

        // Add to world segment as child
        worldSegment.AddChild(stampedShape);

        // Register with rendering system
        _loadedLevelData.AddStampedShape<TestShapeStamp>(worldSegment, stampedShape);
    }
}
