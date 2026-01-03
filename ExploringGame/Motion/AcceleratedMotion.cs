using ExploringGame.Extensions;
using Microsoft.Xna.Framework;

namespace ExploringGame.Motion;

public class AcceleratedMotion
{
    public Vector3 CurrentMotion { get; set; }
    public Vector3 TargetMotion { get; set; }

    // gravity easier to handle separately
    public float TargetY { get; set; }
    public float CurrentY { get; set; }

    public float Acceleration { get; set; }

    public float Gravity { get; set; }

    public void Update()
    {
        if(CurrentMotion != TargetMotion) 
            CurrentMotion = CurrentMotion.MoveToward(TargetMotion, Acceleration);

        if(CurrentY != TargetY)
            CurrentY = CurrentY.MoveToward(TargetY, Gravity);
    }
}
