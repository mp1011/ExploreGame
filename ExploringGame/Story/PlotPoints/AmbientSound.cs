using ExploringGame.Audio;
using Microsoft.Xna.Framework;

namespace ExploringGame.Story.PlotPoints;

public class AmbientSound : PlotPoint
{
    private readonly AudioService _audioService;

    public SoundEffectKey Key { get; }

    public AmbientSound(AudioService audioService, SoundEffectKey key, params PlotPoint[] requiredDone) : base(requiredDone)
    {
        Key = key;
        _audioService = audioService;
    }

    protected override bool CheckActivation(GameTime gameTime) => true;

    protected override void OnActivated()
    {
        _audioService.AddActiveSound(new ActiveAudio(_audioService, SoundEffectKey.CreepyLoop));
    }

    protected override PlotUpdate UpdateActive(GameTime gameTime) => PlotUpdate.End;
}
