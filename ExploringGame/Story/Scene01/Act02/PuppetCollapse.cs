using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.Story.PlotPoints;
using Microsoft.Xna.Framework;

namespace ExploringGame.Story.Scene01.Act02;

public class PuppetCollapse : CharacterAction<Puppet>
{
    public PuppetCollapse(CharacterEntrance<Puppet> characterEntrance, params PlotPoint[] otherRequiredDone) : base(characterEntrance, otherRequiredDone)
    {
    }
    protected override void OnActivated(Puppet shape)
    {
        shape.Controller.Mover.TargetRotation = null;
        shape.Controller.Mover.AbsoluteAngularVelocity = Vector3.Zero;
       
    }

    protected override PlotUpdate UpdateActive(Puppet shape)
    {
        shape.ColliderBodies[0].AddForce(new Jitter2.LinearMath.JVector(15.0f, 0, 32.0f));

        if (shape.Rotation.Yaw.Degrees.Abs() > 70 && shape.Rotation.Pitch.Degrees.Abs() > 70)
            return PlotUpdate.End;
        else 
            return PlotUpdate.Continue;

    }
}
