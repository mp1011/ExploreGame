using ExploringGame.LevelControl;

namespace ExploringGame.Logics.ShapeControllers;

public interface IOnOff
{
    bool On { get; set; }

    public StateKey StateKey { get; }
}

public static class IOnOffExtensions
{
    public static void LoadState(this IOnOff item, GameState state)
    {
        item.On = state.GetBoolean(item.StateKey);
    }

    public static void SaveState(this IOnOff item, GameState state)
    {
        state.Set(item.StateKey, item.On);
    }
}
