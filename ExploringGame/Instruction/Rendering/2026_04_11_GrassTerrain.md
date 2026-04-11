# GOAL
Make `GrassSurface` blade roots follow the `TerrainSurface` height field so that grass blades grow out of the undulating ground instead of floating above or clipping into it.

---

# BACKGROUND

`TerrainSurface` generates its geometry by evaluating a static `SampleNoise(x, z)` method.  
`GrassSurface` currently places every blade root at a fixed `floorY = GetSide(Side.Bottom)`, ignoring the terrain entirely.  
Both shapes are children of the same parent (e.g. `FrontYard`) and share the same world-space `Position` and `Size`, so they already agree on all horizontal bounds and the same `Side.Bottom` reference.

---

# KEY INSIGHT

`TerrainSurface.SampleNoise(x, z)` is the single source of truth for the terrain height at any world-space (x, z) point.  The method is currently `private static`. Making it `internal static` (or extracting it to a shared helper class) means `GrassSurface` can call it directly to resolve the Y coordinate of each blade root without duplicating any logic.

---

# DESIRED CHANGES

## 1. Expose the noise function
**File:** `GeometryBuilder/Shapes/TerrainSurface.cs`

- Change `SampleNoise` from `private static` to `internal static` so it can be called from `GrassSurface`.
- Also expose `AntiClipLift` as `internal static readonly` so callers can replicate the same `baseY` formula:
  ```
  baseY = parentFloor + TerrainSurface.AntiClipLift + TerrainSurface.SampleNoise(x, z)
  ```

## 2. Accept an optional terrain reference in GrassSurface
**File:** `GeometryBuilder/Shapes/GrassSurface.cs`

- Add an optional constructor parameter `TerrainSurface terrain = null`.
- When `terrain != null`, compute each blade root Y as:
  ```
  float rootY = floorY + TerrainSurface.AntiClipLift + TerrainSurface.SampleNoise(x, z);
  ```
  where `x` and `z` are the jittered world-space blade positions already computed in the loop.
- When `terrain == null`, keep the existing behavior (`rootY = floorY`) so that non-yard surfaces are unaffected.
- The blade tips are offset *relative to rootY*, so the two top vertices become:
  ```
  topLeft  = new Vector3(root.X - BladeHalfWidth, rootY + topLeftHeight,  root.Z);
  topRight = new Vector3(root.X + BladeHalfWidth, rootY + topRightHeight, root.Z);
  ```
  No other changes to blade geometry are needed.

## 3. Wire up the terrain in FrontYard and BackYard
**File:** `GeometryBuilder/Shapes/Rooms/ExteriorRooms/FrontYard.cs`  
**File:** `GeometryBuilder/Shapes/Rooms/ExteriorRooms/BackYard.cs`

- Capture the `TerrainSurface` when constructing it and pass it to `GrassSurface`:
  ```csharp
  // Before
  new GrassSurface(this);
  new TerrainSurface(this);

  // After
  var terrain = new TerrainSurface(this);
  new GrassSurface(this, terrain);
  ```
- Apply the same change for each zone where both are used (`northPart` in FrontYard, `this` in BackYard).

---

# FILES TO CHANGE

| File | Change |
|---|---|
| `GeometryBuilder/Shapes/TerrainSurface.cs` | Make `SampleNoise` and `AntiClipLift` `internal static` |
| `GeometryBuilder/Shapes/GrassSurface.cs` | Accept optional `TerrainSurface`; use `SampleNoise` for blade root Y |
| `GeometryBuilder/Shapes/Rooms/ExteriorRooms/FrontYard.cs` | Pass terrain to GrassSurface (×2: main yard + northPart) |
| `GeometryBuilder/Shapes/Rooms/ExteriorRooms/BackYard.cs` | Pass terrain to GrassSurface |

No shader changes are required; the grass blades are authored as plain triangles in `GrassSurface.BuildInternal`, so adjusting their Y position in C# is sufficient.
