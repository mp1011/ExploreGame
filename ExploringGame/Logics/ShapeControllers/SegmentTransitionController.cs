using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ExploringGame.Logics.ShapeControllers;

public class SegmentTransitionController : IShapeController<WorldSegment>
{
    private readonly Player _player;

    public WorldSegment Shape { get; }

    private List<TransitionDetail> _transitions;

    private bool _playerWithinExit;

    private WorldSegment _transitionSegment;

    class TransitionDetail
    {
        public bool PlayerWithinExit { get; set; }
        public Shape ExitShape { get; }
        public WorldSegmentTransition Transition { get; }

        public Axis Axis => Transition.ExitSide.GetAxis();

        public float TransferPosition { get; }

        public TransitionDetail(Shape exitShape, WorldSegmentTransition transition)
        {
            ExitShape = exitShape;
            Transition = transition;

            var size = ExitShape.GetAxisSize(transition.ExitSide.GetAxis());
            TransferPosition = ExitShape.GetSide(transition.ExitSide);

            switch(transition.ExitSide)
            {
                case Side.West:
                case Side.North:
                    TransferPosition += size * 0.4f;
                    break;
                default:
                    TransferPosition -= size * 0.4f;
                    break;
            }
        }
    }

    public SegmentTransitionController(WorldSegment worldSegment, Player player)
    {
        Shape = worldSegment;
        _player = player;
    }

    public void Initialize()
    {
        _transitions = new();
        foreach (var child in Shape.TraverseAllChildren())
        {
            foreach(var transition in Shape.Transitions)
            {
                var shapeType = transition.ShapeType;
                if(child.GetType() == shapeType)
                {
                    _transitions.Add(new TransitionDetail(child, transition));
                }
            }
        }
    }

    public void Update(GameTime gameTime)
    {
        foreach (var transition in _transitions)
        {
            CheckTransition(transition);    
        }
    }

    private void CheckTransition(TransitionDetail transition)
    {
        if(!transition.PlayerWithinExit && transition.ExitShape.ContainsPoint(_player.Position)) 
        {
            transition.PlayerWithinExit = true;
            _transitionSegment = Activator.CreateInstance(transition.Transition.WorldSegmentType) as WorldSegment;
        }

        if(transition.PlayerWithinExit)
        {
            var playerPos = _player.Position.AxisValue(transition.Axis);
            switch(transition.Transition.ExitSide)
            {
                case Side.West:
                case Side.North:
                    if (playerPos < transition.TransferPosition)
                        ActivateTransition(transition);
                    break;
                default:
                    if (playerPos > transition.TransferPosition)
                        ActivateTransition(transition);
                    break;
            }
        }
    }

    private void ActivateTransition(TransitionDetail transition)
    {
        throw new System.NotImplementedException();
    }
}
