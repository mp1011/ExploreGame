using ExploringGame.Extensions;
using Microsoft.Xna.Framework;

namespace ExploringGame.Motion;

public class AcceleratedMotion
{
    public Vector3 CurrentMotion { get; set; }
    public Vector3 TargetMotion { get; set; }

    public float Acceleration { get; set; }

    public void Update()
    {
        if(CurrentMotion == TargetMotion) 
            return;

        CurrentMotion = CurrentMotion.MoveToward(TargetMotion, Acceleration);
    }
}
