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
    BedroomClosetDoorOpen,
    KidsBedroomDoorOpen,
    KidsBedroomClosetDoorOpen,
    LinenClosetDoorOpen,
    SpareRoomDoorOpen,
    SpareRoomClosetDoorOpen,
    HallLightOn,
    BasementLightOn,
    DenDoorsOpen,
    BathroomDoorOpen, 
    HalfBathroomDoorOpen,
    DenClosetDoorOpen,
    KitchenLightOn,
    LivingRoomLightOn,
    DenLightOn,
    BathroomLightOn,
    LeftBedroomLightOn,
    RightBedroomLightOn,
    HalfBathroomLightOn,
    SpareRoomLightOn,
    KidsBedroomLightOn,
    GarageInnerDoorOpen,
    FrontDoorOpen,
    GarageDoor1Open,
    GarageDoor2Open,
    DeckSlidingDoorOpen
}

public class GameState
{
    private Dictionary<StateKey, int> _values = new();
    

    public GameState()
    {        
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
