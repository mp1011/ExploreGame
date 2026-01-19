using ExploringGame.Config;
using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace ExploringGame.Logics.ShapeControllers;

public class SegmentTransitionController : IShapeController<WorldSegment>
{
    private readonly LoadedLevelData _renderBuffers;
    private readonly Player _player;
    private readonly TransitionShapesRegistrar _transitionShapesRegistrar;

    public WorldSegment Shape { get; }

    private TransitionDetail[] _transitions;

    private bool _playerWithinExit;

    private WorldSegment _transitionSegment;

    class TransitionDetail
    {
        public bool PlayerWithinExit { get; set; }
        public Shape ExitShape { get; }
        public WorldSegmentTransition Transition { get; }

        public Axis Axis => Transition.ExitSide.GetAxis();

        public float TransferPosition { get; }

        public TransitionDetail(WorldSegmentTransition transition)
        {
            ExitShape = transition.ExitShape;
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

    public SegmentTransitionController(WorldSegment worldSegment, Player player, 
        TransitionShapesRegistrar transitionShapesRegistrar, LoadedLevelData renderBuffers)
    {
        Shape = worldSegment;
        _player = player;
        _transitionShapesRegistrar = transitionShapesRegistrar;
        _renderBuffers = renderBuffers;
    }

    public void Initialize()
    {
        _transitions = Shape.Transitions.Select(p => new TransitionDetail(p)).ToArray();
    }

    public void Stop()
    {
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
        //if(!transition.PlayerWithinExit && transition.ExitShape.ContainsPoint(_player.Position)) 
        //{
        //    transition.PlayerWithinExit = true;
        //    _transitionSegment = Activator.CreateInstance(transition.Transition.WorldSegmentType, _transitionShapesRegistrar) as WorldSegment;
        //    _renderBuffers.PrepareNextSegment(_transitionSegment);
        //}

        //if(transition.PlayerWithinExit)
        //{
        //    if(!transition.ExitShape.ContainsPoint(_player.Position))
        //    {
        //        transition.PlayerWithinExit = false;
        //        return;
        //    }

        //    var playerPos = _player.Position.AxisValue(transition.Axis);
        //    switch(transition.Transition.ExitSide)
        //    {
        //        case Side.West:
        //        case Side.North:
        //            if (playerPos < transition.TransferPosition)
        //                ActivateTransition(transition);
        //            break;
        //        default:
        //            if (playerPos > transition.TransferPosition)
        //                ActivateTransition(transition);
        //            break;
        //    }
        //}
    }

    private void ActivateTransition(TransitionDetail transition)
    {
        // todo, this needs more
        _renderBuffers.SwapActive();

        _transitions = Array.Empty<TransitionDetail>();
    }
}
