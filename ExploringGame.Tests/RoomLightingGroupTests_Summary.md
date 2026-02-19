# Room Lighting Group Tests Summary

This document summarizes the failing unit tests created for the shader rework implementation.

## Test File
`ExploringGame.Tests\RoomLightingGroupTests.cs`

## Compilation Status
✅ Tests compile with expected failures (missing methods/properties to be implemented)

## Tests by Task

### Task 1: RoomLightingCalculator - Calculate by LightingGroup

**Tests:**
1. `RoomLightingCalculator_GroupsRoomsByLightingGroup()`
   - Verifies calculator can get all distinct lighting groups
   - **Missing:** `calculator.GetDistinctLightingGroups()` method

2. `RoomLightingCalculator_CalculatesLightForLightingGroups()`
   - Verifies calculator can retrieve light data for a lighting group
   - **Missing:** `calculator.GetLightDataForGroup(Room lightingGroup)` method

3. `RoomLightingCalculator_RoomsInSameLightingGroupShareLightData()`
   - Verifies rooms with same LightingGroup return the same RoomLightData instance
   - **Missing:** `calculator.GetLightDataForGroup(Room lightingGroup)` method

### Task 2: ShapeBufferCreator - One Buffer per LightingGroup and Texture Sheet

**Tests:**
1. `ShapeBufferCreator_CreatesOneBufferPerLightingGroupAndTextureSheet()`
   - Verifies each LightingGroup+TextureSheet combination has exactly one buffer
   - **Missing:** `ShapeBuffer.LightingGroup` property
   - **Missing:** `ShapeBuffer.TextureSheetKey` property (or adjust to use existing Texture)

2. `ShapeBufferCreator_IncludesRoomAndChildrenInLightingGroupBuffer()`
   - Verifies buffer includes room, rooms that reference it as LightingGroup, and children
   - **Missing:** `ShapeBuffer.LightingGroup` property

3. `ShapeBufferCreator_CreatesBufferForRemainingStaticShapes()`
   - Verifies a buffer exists for shapes not in any lighting group
   - **Missing:** `ShapeBuffer.IsRemainingStaticShapesBuffer` property

### Task 3: ShapeBufferCreator - Add RoomLightData

**Tests:**
1. `ShapeBufferCreator_HasRoomLightDataProperty()`
   - Integration test to verify ShapeBufferCreator can access RoomLightData
   - This is more of a placeholder - actual verification will happen in implementation

### Task 4: RoomLightData - Cached Total Light Level

**Tests:**
1. `RoomLightData_TotalLightIsProperty()`
   - Verifies TotalLight is accessible as a property (not method)
   - **Missing:** `RoomLightData.TotalLight` property (currently is `GetTotalLight()` method)

2. `RoomLightData_TotalLightNotRecalculatedOnEveryAccess()`
   - Verifies accessing TotalLight multiple times returns the same cached value
   - **Missing:** `RoomLightData.TotalLight` property

3. `RoomLightData_HasRecalculateLightLevelMethod()`
   - Verifies RecalculateLightLevel method exists
   - **Missing:** `RoomLightData.RecalculateLightLevel()` method

4. `RoomLightData_RecalculateLightLevelUpdatesCache()`
   - Verifies calling RecalculateLightLevel updates the cached TotalLight value
   - **Missing:** `RoomLightData.TotalLight` property
   - **Missing:** `RoomLightData.RecalculateLightLevel()` method

5. `RoomLightingCalculator_CallsRecalculateLightLevelOnLightStateChange()`
   - Verifies calculator calls RecalculateLightLevel when a light is toggled
   - **Missing:** `RoomLightData.TotalLight` property
   - Tests integration with light state changes

6. `RoomLightingCalculator_CallsRecalculateLightLevelOnDoorStateChange()`
   - Verifies calculator calls RecalculateLightLevel when a door is opened/closed
   - **Missing:** `RoomLightData.TotalLight` property
   - Tests integration with door state changes

## Missing Members Summary

### RoomLightingCalculator
- `IEnumerable<Room> GetDistinctLightingGroups()`
- `RoomLightData GetLightDataForGroup(Room lightingGroup)`

### ShapeBuffer (record)
- `Room LightingGroup { get; init; }` (or as constructor parameter)
- `bool IsRemainingStaticShapesBuffer { get; init; }` (or as constructor parameter)
- Note: May need to use `Texture` instead of adding `TextureSheetKey` if not already present

### RoomLightData
- `float TotalLight { get; }` property (replace `GetTotalLight()` method)
- `void RecalculateLightLevel()` method

### ShapeBufferCreator (Task 3)
- Needs access to RoomLightData (likely via dependency injection or parameter)

## Next Steps
Once you approve these tests, the implementation should:
1. Add the missing methods/properties to pass compilation
2. Implement the logic to make the tests pass
3. Run the tests to verify the implementation works correctly
