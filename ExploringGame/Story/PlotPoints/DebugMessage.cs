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
            Message = "One Two Three Four Five Six Seven Eight Nine Ten 1One 1Two 1Three 1Four 1Five 1Six 1Seven 1Eight 1Nine 1Ten 2One 2Two 2Three 2Four 2Five 2Six 2Seven 2Eight 2Nine 2Ten";
        }

        public override PlotUpdate UpdateActive(GameTime gameTime)
        {
            _dialogueManager.Enqueue(new DialogueEntry(_actor, Message));
            return PlotUpdate.End;
        }
    }
}
