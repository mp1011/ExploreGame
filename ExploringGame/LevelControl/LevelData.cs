using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Logics.Pathfinding;
using ExploringGame.Logics.ShapeControllers;
using ExploringGame.Rendering;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.LevelControl;

public class LevelData
{
    public IActiveObject[] ActiveObjects { get; }

    /// <summary>All shape buffers grouped by the render pass that owns them.</summary>
    public Dictionary<IRenderPass, List<ShapeBuffer>> BuffersByPass { get; } = new();

    /// <summary>
    /// Stamp-template buffers indexed by stamp type. Not included in BuffersByPass
    /// since they are templates shared by StampedShape instances.
    /// </summary>
    public Dictionary<Type, ShapeBuffer> StampShapeBuffers { get; } = new();

    /// <summary>
    /// Backward-compatible view: all opaque (non-stamp) buffers.
    /// Equivalent to BuffersByPass[opaquePass] minus stamped shapes.
    /// </summary>
    public ShapeBuffer[] ShapeBuffers
    {
        get
        {
            if (_opaquePass == null || !BuffersByPass.TryGetValue(_opaquePass, out var list))
                return Array.Empty<ShapeBuffer>();
            return list.Where(b => b.Shape is not StampedShape).ToArray();
        }
    }

    /// <summary>
    /// Backward-compatible view: dynamically added stamped-shape buffers.
    /// These live in BuffersByPass[opaquePass] alongside regular opaque geometry.
    /// </summary>
    public IReadOnlyList<ShapeBuffer> StampedShapeBuffers
    {
        get
        {
            if (_opaquePass == null || !BuffersByPass.TryGetValue(_opaquePass, out var list))
                return Array.Empty<ShapeBuffer>();
            return list.Where(b => b.Shape is StampedShape).ToList();
        }
    }

    public bool Initialized { get; private set; }
    public WorldSegment WorldSegment { get; }

    private readonly IRenderPass _opaquePass;

    public LevelData(WorldSegment worldSegment, ShapeBuffer[] allShapeBuffers, IActiveObject[] activeObjects,
        RenderPassRegistry registry = null)
    {
        WorldSegment = worldSegment;
        ActiveObjects = activeObjects.ToArray();
        Initialized = false;
        _opaquePass = registry?.CatchAllPass;

        // Sort each incoming buffer into BuffersByPass (stamp templates go in StampShapeBuffers)
        foreach (var buffer in allShapeBuffers)
        {
            if (buffer.Shape is ShapeStamp)
            {
                StampShapeBuffers[buffer.Shape.GetType()] = buffer;
                continue;
            }

            var pass = buffer.RenderPass ?? _opaquePass;
            if (pass == null)
                continue;

            if (!BuffersByPass.ContainsKey(pass))
                BuffersByPass[pass] = new List<ShapeBuffer>();
            BuffersByPass[pass].Add(buffer);
        }
    }

    public void Initialize()
    {
        if (Initialized)
            return;

        foreach (var obj in ActiveObjects)
            obj.Initialize();

        Initialized = true;
    }

    public void Stop()
    {
        if (!Initialized)
            return;

        foreach (var obj in ActiveObjects)
            obj.Stop();

        Initialized = false;
    }

    public void Update(GameTime gameTime)
    {
        foreach (var obj in ActiveObjects)
            obj.Update(gameTime);
    }

    public void AddStampedShape<TStamp>(StampedShape<TStamp> stampedShape,
        RoomGraph roomGraph = null,
        RoomLightingCalculator lightingCalculator = null)
        where TStamp : ShapeStamp
    {
        var stampType = typeof(TStamp);

        if (!StampShapeBuffers.TryGetValue(stampType, out var stampBuffer))
        {
            throw new InvalidOperationException($"No ShapeStamp of type {stampType.Name} found in this WorldSegment");
        }

        // Detect which room the stamped shape is in
        Room room = null;
        if (roomGraph != null && stampedShape is IPlaceableObject placeableShape)
        {
            room = FindRoomContainingPosition(stampedShape.Position, roomGraph);
            placeableShape.Room = room;
        }

        Room lightingGroup = room?.LightingGroup;

        var pass = stampBuffer.RenderPass ?? _opaquePass;

        var stampedBuffer = new ShapeBuffer(
            stampedShape,
            stampBuffer.VertexBuffer,
            stampBuffer.IndexBuffer,
            stampBuffer.TriangleCount,
            stampBuffer.Texture,
            pass,
            stampBuffer.RasterizerState,
            lightingGroup
        );

        if (pass != null)
        {
            if (!BuffersByPass.ContainsKey(pass))
                BuffersByPass[pass] = new List<ShapeBuffer>();
            BuffersByPass[pass].Add(stampedBuffer);
        }
    }

    private Room FindRoomContainingPosition(Vector3 position, RoomGraph roomGraph)
    {
        foreach (var room in roomGraph.GetAllRooms())
        {
            if (room.ContainsPoint(position))
                return room;
        }

        Room nearestRoom = null;
        float nearestDistance = float.MaxValue;

        foreach (var room in roomGraph.GetAllRooms())
        {
            var distance = Vector3.DistanceSquared(position, room.Position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestRoom = room;
            }
        }

        return nearestRoom;
    }
}
