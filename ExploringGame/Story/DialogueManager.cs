using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ExploringGame.Story;

public class DialogueManager
{
    private Queue<DialogueEntry> _lines = new();

    public void Enqueue(DialogueEntry entry) => _lines.Enqueue(entry);

    public void Update(GameTime gameTime)
    {

    }
}
