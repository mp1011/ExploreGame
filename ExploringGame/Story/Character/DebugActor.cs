using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;

namespace ExploringGame.Story.Character;

public class DebugActor : StoryActor
{
    public override string Name => "Debug";

    public override Color TextColor => Color.Red;

    public override HAlign TextAlign => HAlign.Right;
}
