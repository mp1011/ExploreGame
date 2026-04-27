using ExploringGame.Story.Character;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.Story.PlotPoints
{
    class DebugMessage : PlotPoint
    {
        private readonly DialogueManager _dialogueManager;
        private readonly StoryActor _actor;

        public string Message { get; }
        public DebugMessage(DialogueManager dialogueManager, DebugActor debugActor) : base(Array.Empty<PlotPoint>())
        {
            _dialogueManager = dialogueManager;
            _actor = debugActor;
            Message = "Hello World";
        }

        public override PlotUpdate UpdateActive(GameTime gameTime)
        {
            _dialogueManager.Enqueue(new DialogueEntry(_actor, Message));
            return PlotUpdate.End;
        }
    }
}
