using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;

namespace ExploringGame.Story.Character;

public abstract class StoryActor
{
    public abstract string Name { get; }
    public abstract Color TextColor { get;  }

    public abstract HAlign TextAlign { get;}
}
