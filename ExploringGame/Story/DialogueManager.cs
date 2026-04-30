using ExploringGame.Logics;
using ExploringGame.Logics.Controllers;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Story;

public record Word(string[] Letters, Color Color)
{
    public Word(string text, Color color) : this(text.ToCharArray().Select(p=>p.ToString()).ToArray(), color)
    {}
}

public class DialogueManager
{
    private static readonly TimeSpan NextLetterTime = TimeSpan.FromMilliseconds(20);
    private readonly IPlayerInput _playerInput;
    private readonly AudioService _audio;

    private Queue<DialogueEntry> _lines = new();
    private DialogueEntry _current;

    private TimedAction _nextLetter;

    private Word[] _currentWords = null;
    private int _wordIndex = 0;
    private int _letterIndex = 0;
    private bool _limitedReached;

    public bool HasDialogue => _currentWords != null && _currentWords.Length > 0;

    public IEnumerable<Word> PreviousWords => _currentWords.Take(_wordIndex);

    public Word CurrentWord => _currentWords[_wordIndex];
    public int LetterIndex => _letterIndex;

    public void Enqueue(DialogueEntry entry) => _lines.Enqueue(entry);
    public void EnqueueIfNeeded(DialogueEntry entry)
    {
        if (_current == entry)
            return;

        if (_lines.Any(p => p.Equals(entry)))
            return;

        _lines.Enqueue(entry);
    }

    public DialogueManager(IPlayerInput playerInput, AudioService audioService)
    {
        _audio = audioService;
        _playerInput = playerInput;
        _nextLetter = new TimedAction(NextLetterTime, NextLetter);
    }

    public void LimitReached(int limitWord=-1)
    {
        _limitedReached = true;
        if (limitWord > -1)
            _wordIndex = limitWord;
    }

    private void NextLetter()
    {
        if (_currentWords == null)
            return;

        var word = _currentWords[_wordIndex];

        var newLetterIndex = _letterIndex + 1;
        int newWordIndex = _wordIndex;

        if (newLetterIndex >= word.Letters.Length)
        {
            newWordIndex++;
            newLetterIndex = 0;
        }
        
        if(newWordIndex >= _currentWords.Length)
        {
            newWordIndex = _currentWords.Length - 1;
            newLetterIndex = word.Letters.Length - 1;
        }

        if (newLetterIndex > _letterIndex)
            _audio.Play(SoundEffectKey.TextBeep);

        _letterIndex = newLetterIndex;
        _wordIndex = newWordIndex;
    }

    public void Update(GameTime gameTime)
    {
        if (!HasDialogue && _lines.Any())
            NextDialogue();

        if (_limitedReached)
        {
            if (_playerInput.IsKeyPressed(GameKey.DialogueAdvance))
            {
                if (AtEndOfDialogue())
                    NextDialogue();
                else
                    ContinueDialogue();
            }
        }
        else if(HasDialogue)
        {
            _nextLetter.Update(gameTime);

            if (_playerInput.IsKeyPressed(GameKey.DialogueAdvance))
            {
                if (AtEndOfDialogue())
                    NextDialogue();
                else 
                    FastForwardDialogue();
            }
        }
    }

    private void NextDialogue()
    {
        _limitedReached = false;
        if(!_lines.Any())
        {
            _currentWords = Array.Empty<Word>();
            _current = null;
            _wordIndex = 0;
            _letterIndex = 0;
            return;
        }

        var entry = _lines.Dequeue();
        _current = entry;
        _currentWords = LoadWords(entry).ToArray();
        _wordIndex = 0;
        _letterIndex = 0;
    }

    private void FastForwardDialogue()
    {
        _letterIndex = 1000;
        _wordIndex = _currentWords.Length - 1;
    }

    private bool AtEndOfDialogue()
    {
        return _wordIndex == _currentWords.Length - 1 && _letterIndex >= CurrentWord.Letters.Length-1;
    }

    private void ContinueDialogue()
    {
        _currentWords = _currentWords.Skip(_wordIndex).ToArray();
        _wordIndex = 0;
        _letterIndex = 0;
        _limitedReached = false;     
    }

    private static IEnumerable<Word> LoadWords(DialogueEntry dialogueEntry)
    {
        return dialogueEntry.Line.Split(' ', System.StringSplitOptions.RemoveEmptyEntries)
                                 .Select(p => new Word(p, dialogueEntry.Actor.TextColor));
    }
}
