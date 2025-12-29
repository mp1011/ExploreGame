using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace ExploringGame.Logics;

internal class EntityCollider
{
    private Shape[] _collidableShapes = Array.Empty<Shape>();
    private Vector3 _lastValidPosition;

    private Shape _currentRoom;
    public Shape CurrentRoom
    {
        get => _currentRoom;
        set
        {
            _currentRoom = value;
            _collidableShapes = GetCollidableShapes();
        }
    }

    public IWithPosition Entity { get; set; }

    public void Update()
    {
        foreach(var shape in _collidableShapes)
            CheckCollision(shape);

        _lastValidPosition = Entity.Position;
    }

    private void CheckCollision(Shape shape)
    {
        var position = Entity.Position;

        // super basic collision, to be improved later
        if (shape.ViewFrom == ViewFrom.Inside)
        {
            
            float padding = 0.2f;
            position.X = MathHelper.Clamp(position.X, CurrentRoom.GetSide(Side.West) + padding, CurrentRoom.GetSide(Side.East) - padding);
            position.Z = MathHelper.Clamp(position.Z, CurrentRoom.GetSide(Side.North) + padding, CurrentRoom.GetSide(Side.South) - padding);
            
        }
        else if(shape.ViewFrom == ViewFrom.Outside)
        {
            var collisionArea = new RectangleF(shape.GetSide(Side.West), shape.GetSide(Side.North), shape.Width, shape.Depth);

            if(collisionArea.Contains(Entity.TopDownPosition()))
            {
                position = _lastValidPosition;
            }
        }

        Entity.Position = position;
    }


    private Shape[] GetCollidableShapes() => CurrentRoom.TraverseAllChildren().Where(p => p.CollisionEnabled).ToArray();
}
