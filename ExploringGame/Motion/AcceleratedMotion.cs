using ExploringGame.Extensions;
using Microsoft.Xna.Framework;

namespace ExploringGame.Motion;

public class AcceleratedMotion
{
    public Vector3 CurrentMotion { get; set; }
    public Vector3 TargetMotion { get; set; }

    public float CurrentY
    {
        get => CurrentMotion.Y;
        set => CurrentMotion = new Vector3(CurrentMotion.X, value, CurrentMotion.Z);        
    }

    public float TargetY
    {
        get => TargetMotion.Y;
        set => TargetMotion = new Vector3(TargetMotion.X, value, TargetMotion.Z);
    }

    public float Acceleration { get; set; }

    public void Update()
    {
        if(CurrentMotion == TargetMotion) 
            return;

        CurrentMotion = CurrentMotion.MoveToward(TargetMotion, Acceleration);
    }
}
