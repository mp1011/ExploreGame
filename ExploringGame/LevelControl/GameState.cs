using System.Collections.Generic;

namespace ExploringGame.LevelControl;


public enum StateKey
{
    None,
    OfficeDoor1Open,
    OfficeDoor2Open,
    OfficeDoor3Open,
    OfficeLightOn,
    BasementStairsDoorOpen,
    BedroomDoorOpen,
    KidsBedroomDoorOpen,
    LinenClosetDoorOpen,
    SpareRoomDoorOpen,
    HallLightOn,
    BasementLightOn,
    DenDoorsOpen,
    BathroomDoorOpen, 
    HalfBathroomDoorOpen,
    DenClosetDoorOpen
}

public class GameState
{
    private Dictionary<StateKey, int> _values = new();
    

    public GameState()
    {
        Set(StateKey.OfficeLightOn, true);
        Set(StateKey.HallLightOn, true);
        Set(StateKey.BasementLightOn, true);
    }

    public int Get(StateKey key)
    {
        if (key == StateKey.None)
            return 0;

        if(_values.TryGetValue(key, out int value)) return value;

        return 0;
    }

    public bool GetBoolean(StateKey stateKey) => Get(stateKey) != 0;

    public void Set(StateKey key, int value)
    {
        if (key == StateKey.None)
            throw new System.Exception("Invalid key");
        _values[key] = value;
    }

    public void Set(StateKey key, bool value) => Set(key, value ? 1 : 0);

}
