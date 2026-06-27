using ExploringGame.Logics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ExploringGame.Story.PlotPoints;

public class ActorPose<TActor,TPose> : PlotPoint
    where TActor:IPhysicsShape
    where TPose:ActorPose
{
    private TPose _pose;

    public ActorPose(TPose pose, IEnumerable<PlotPoint> requiredDone) : base(requiredDone)
    {
    }

    protected override bool CheckActivation(GameTime gameTime)
    {
        throw new System.NotImplementedException();
    }

    protected override PlotUpdate UpdateActive(GameTime gameTime)
    {
        throw new System.NotImplementedException();
    }
}

public abstract record ActorPose
{

}

public record LieDownPose : ActorPose
{

}