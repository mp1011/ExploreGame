using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;

namespace ExploringGame.Services;


public enum SoundEffectKey
{
     DoorOpen,
     DoorClose
}
public class AudioService
{

    private Dictionary<SoundEffectKey, SoundEffect> _effects = new();

    public void LoadContent(ContentManager contentManager)
    {
        _effects.Clear();

        foreach(SoundEffectKey key in Enum.GetValues(typeof(SoundEffectKey)))
        {
            _effects[key] = contentManager.Load<SoundEffect>($"Sound/{key}");
        }        
    }

    public void Play(SoundEffectKey key)
    {
        _effects[key].Play();
    }
}
