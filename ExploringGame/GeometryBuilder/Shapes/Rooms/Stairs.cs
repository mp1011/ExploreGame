using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics.Collision;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms;

public abstract class Stairs : Room
{
    protected Vector3 StepSize  { get; }
    protected Shape BottomFloor { get; }

    protected Shape TopFloor { get; }

    private StairStep[] _steps;
    protected abstract Side StartSide { get; }

    public Stairs(WorldSegment worldSegment, Vector2 stepSize, Shape bottomFloor, Shape topFloor, float width, float depth) : base(worldSegment)
    {
        Width = width;
        Depth = depth;
        BottomFloor = bottomFloor;
        TopFloor = topFloor;
        StepSize = new Vector3(stepSize.X, 0, stepSize.Y); // required to compute step size Y
        StepSize = new Vector3(stepSize.X, CalcStepHeight(), stepSize.Y);
        Height = CalcHeight();
    }

    protected abstract StairStep CreateStep();

    private float CalcStepHeight()
    {
        var heightDifference = TopFloor.GetSide(Side.Bottom) - BottomFloor.GetSide(Side.Bottom);
        return heightDifference / (float)NumSteps;
    }

    private float CalcHeight()
    {
        return TopFloor.GetSide(Side.Top) - BottomFloor.GetSide(Side.Bottom);
    }

    private int NumSteps
    {
        get
        {
            var neededSize = this.GetAxisSize(StartSide.GetAxis());
            var sizePerStep = StepSize.AxisValue(StartSide.GetAxis());
            if (sizePerStep == 0f)
                throw new ArgumentException("Invalid Step Size");

            var numSteps = (int)Math.Ceiling(neededSize / sizePerStep);
            if (numSteps == 0)
                throw new ArgumentException("Invalid Step Size");

            return numSteps;
        }
    }

    private void CreateAllSteps()
    {       
        _steps = Enumerable.Range(0, NumSteps).Select(p => AddChild(CreateStep())).ToArray();
    }

    protected override void BeforeBuild()
    {
        if (_steps == null)
            CreateAllSteps();

        var stairPosition = GetSide(StartSide);

        Shape lastStep = null ;
        foreach (var step in _steps)
        {
            step.Position = Position;
            step.Size = StepSize;
            step.SetSide(StartSide, stairPosition);
            step.Place().OnFloor();

            if (lastStep != null ) 
                step.SetSideUnanchored(Side.Top, lastStep.GetSide(Side.Top) + StepSize.Y);
           
            stairPosition = step.GetSide(StartSide.Opposite());
            lastStep = step;
        }

        _steps[^1].SetSideUnanchored(StartSide.Opposite(), TopFloor.GetSide(StartSide));
    }
}


public class StairStep : Shape, ICollidable
{
    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public override IColliderMaker ColliderMaker => ColliderMakers.Step(this);

    public CollisionGroup CollisionGroup => CollisionGroup.Steps;

    public CollisionGroup CollidesWithGroups => CollisionGroup.MovingObjects;

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }
}