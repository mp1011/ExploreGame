using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace ExploringGame.Logics.Collision;

public class EntityCollider
{
    private Shape[] _collidableShapes = Array.Empty<Shape>();
    private Vector3 _lastValidPosition;
    public WorldSegment CurrentWorldSegment { get; set; }

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
        if (CurrentRoom == null)
            CurrentRoom = CalcCurrentRoom();

        CollisionResponse collisionResponse = CollisionResponse.None(Entity.Position);
        foreach(var shape in _collidableShapes)
            collisionResponse = CheckCollision(shape, collisionResponse);

        _lastValidPosition = Entity.Position;
    }

    private Shape CalcCurrentRoom()
    {
        return CurrentWorldSegment.Children.FirstOrDefault(p => p.ContainsPoint(Entity.Position));
    }

    private CollisionResponse CheckCollision(Shape shape, CollisionResponse lastResponse)
    {
        var position = lastResponse.NewPosition;

        // super basic collision, to be improved later
        if(shape is ICollisionResponder collisionResponder)
        {
            var response = collisionResponder.CheckCollision(lastResponse);
            position = response.NewPosition;
            Entity.Position = position;

            if (response.IgnoreWallCollision)
            {
                // todo, not the right place for this
                CurrentRoom = CalcCurrentRoom();
                return response;
            }
        }
        else if (!lastResponse.IgnoreWallCollision && shape.ViewFrom == ViewFrom.Inside)
        {
            position = CheckWallCollision(shape, position);            
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
        return new CollisionResponse(
            OriginalPosition: lastResponse.OriginalPosition, 
            NewPosition: position, 
            IgnoreWallCollision: lastResponse.IgnoreWallCollision);
    }

    private Vector3 CheckWallCollision(Shape shape, Vector3 position)
    {
        if (shape == CurrentRoom)
        {
            float padding = 0.2f;
            position.X = MathHelper.Clamp(position.X, CurrentRoom.GetSide(Side.West) + padding, CurrentRoom.GetSide(Side.East) - padding);
            position.Z = MathHelper.Clamp(position.Z, CurrentRoom.GetSide(Side.North) + padding, CurrentRoom.GetSide(Side.South) - padding);
        }

        return position;
    }

    private Shape[] GetCollidableShapes()
    {
        var shapes = CurrentRoom.TraverseAllChildren().Where(p => p.CollisionEnabled).ToArray();

        // collision responders to the front the list
        shapes = shapes.OrderBy(p => p is ICollisionResponder ? 0 : 1).ToArray();

        return shapes;
    }
}
