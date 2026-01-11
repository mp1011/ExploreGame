using ExploringGame.Extensions;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms;

public abstract class Stairs : Room
{
    public  Vector3 StepSize  { get; }

    private Shape[] _steps;
    protected abstract Side StartSide { get; }

    public Stairs(WorldSegment worldSegment, Vector3 stepSize) : base(worldSegment)
    {
        StepSize = stepSize;
    }

    protected abstract Shape CreateStep();

    private void CreateAllSteps()
    {
        var neededSize = this.GetAxisSize(StartSide.GetAxis());
        var sizePerStep = StepSize.AxisValue(StartSide.GetAxis());

        int steps = (int)Math.Ceiling(neededSize / sizePerStep);
        _steps = Enumerable.Range(0, steps).Select(p => AddChild(CreateStep())).ToArray();
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
    }


    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return Array.Empty<Triangle>();
    }
}
