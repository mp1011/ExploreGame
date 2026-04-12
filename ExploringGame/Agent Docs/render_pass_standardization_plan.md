# Standardizing Custom Shader / Render Passes

## Problem

Every new special-purpose shader (grass, skybox, glass, etc.) currently requires changes to **five separate places**:

| File | What changes |
|---|---|
| `Rendering/ShapeBuffer.cs` | Add a new `ShapeBufferType` enum value |
| `LevelControl/LevelData.cs` | Add a new `List<ShapeBuffer>` property and filter in the constructor |
| `Services/ShapeBufferCreator.cs` | Add a new `Create*ShapeBuffer` private method and special-case detection |
| `LevelControl/LoadedLevelData.cs` | Sometimes needs wiring too (e.g. `SkyboxBuffer`) |
| `Game1.cs` | Add a new `IRenderEffect` field, wire it in `LoadContent`, and add a draw call in `DrawWorld` |

The goal is to add a new shader type by touching **one file** — the render pass class itself.

---

## Core Idea: `IRenderPass`

Replace the `ShapeBufferType` enum pattern with a **render-pass registry**. Each special shader is an `IRenderPass` that:

1. Declares which shapes it owns.
2. Knows how to build a `ShapeBuffer` for those shapes.
3. Knows how to draw its buffers.
4. Declares its draw order relative to other passes.

`Game1` registers passes once at startup. Every downstream system (`ShapeBufferCreator`, `LevelData`, `DrawWorld`) operates generically over the registry.

---

## New Abstractions

### `Rendering/IRenderPass.cs`

```
interface IRenderPass
    int DrawOrder                             // controls draw sequence; lower = earlier
    bool ClaimsShape(Shape shape)             // called by ShapeBufferCreator to route shapes
    ShapeBuffer BuildBuffer(Shape shape, ...)  // how to build a ShapeBuffer for a claimed shape
    void Draw(GraphicsDevice, IReadOnlyList<ShapeBuffer>, Matrix view, Matrix projection)
    void LoadContent(Game game, LoadedTextureSheets textures)
```

`DrawOrder` constants:
- 0  = opaque geometry (current `TwoPassRenderEffect`)  
- 10 = grass  
- 50 = transparent / glass  
- 90 = skybox  
- 100 = post-process / HUD

### `Rendering/RenderPassRegistry.cs`

Simple sorted list of `IRenderPass` objects.

```
class RenderPassRegistry
    IReadOnlyList<IRenderPass> Passes  // sorted by DrawOrder
    void Register(IRenderPass pass)
```

Registered once in `Game1.LoadContent` (or `Initialize`).

### Existing effects become `IRenderPass` implementations

| Current class | Becomes |
|---|---|
| `TwoPassRenderEffect` | `OpaqueRenderPass : IRenderPass` — `ClaimsShape` returns true for all shapes not claimed by another pass |
| `GrassRenderEffect` | `GrassRenderPass : IRenderPass` — claims `GrassSurface` shapes |
| `SkyboxRenderEffect` | `SkyboxRenderPass : IRenderPass` — claims `SkyboxShape` shapes |
| _(future)_ `GlassRenderEffect` | `GlassRenderPass : IRenderPass` — claims `GlassPane` shapes |

The default `OpaqueRenderPass` acts as a catch-all: `ClaimsShape` returns true for anything no earlier pass claimed.

---

## Changes to Existing Files

### `Rendering/ShapeBuffer.cs`
- **Remove** `ShapeBufferType` enum entirely.
- The `ShapeBuffer` record no longer needs a `Type` field; the pass that built the buffer is the source of truth.

### `Services/ShapeBufferCreator.cs`
- Accept `RenderPassRegistry` as a constructor parameter.
- Replace the hard-coded `if (grassSurfaces.Any())` / `CreateGrassShapeBuffer` / `CreateSkyboxBuffer` branches with a loop:
  ```
  foreach shape in allShapes:
      pass = registry.Passes.FirstOrDefault(p => p.ClaimsShape(shape)) ?? opaquePass
      yield return pass.BuildBuffer(shape, ...)
  ```
- The per-pass buffer-building logic moves **into each pass** (`BuildBuffer`), eliminating the need for `ShapeBufferCreator` to know about any specific type.

### `LevelControl/LevelData.cs`
- Replace the named per-type lists (`GrassShapeBuffers`, and any future ones) with a single:
  ```csharp
  Dictionary<IRenderPass, List<ShapeBuffer>> BuffersByPass { get; }
  ```
- The constructor loops over the incoming buffers and groups them by which pass owns them (determined via `pass.ClaimsShape(buffer.Shape)` or by storing the pass reference on `ShapeBuffer` — see note below).
- `ShapeBuffers` (opaque, normal) becomes `BuffersByPass[opaquePass]` — no named property needed.
- Removing a special type in the future means deleting its `IRenderPass` class; `LevelData` needs no change.

> **Note on ownership tagging:** Two approaches work equally well:
> - Store the owning `IRenderPass` reference directly on `ShapeBuffer` (add one field).
> - Re-run `ClaimsShape` during `LevelData` construction. Either is fine; the first avoids re-scanning.

### `LevelControl/LoadedLevelData.cs`
- Remove `SkyboxBuffer` special property; the skybox buffer lives in `BuffersByPass[skyboxPass]` inside the relevant `LevelData`.
- `LoadSegment` no longer needs the special skybox-building branch; `ShapeBufferCreator` handles it via the `SkyboxRenderPass`.

### `Game1.cs`
- `LoadContent`: replace individual effect fields with a `RenderPassRegistry`. Register each pass, call `pass.LoadContent(this, loadedTextures)`.
- `DrawWorld`: replace named draw calls with a single loop:
  ```csharp
  foreach (var pass in _renderPassRegistry.Passes)
  {
      foreach (var levelData in _loadedLevelData.ActiveSegments)
      {
          var buffers = levelData.BuffersByPass.GetValueOrDefault(pass);
          if (buffers?.Count > 0)
              pass.Draw(graphicsDevice, buffers, view, projection);
      }
  }
  ```
- `Game1` no longer has per-shader fields (`_renderEffect`, `_grassRenderEffect`, `_skyboxEffect`). Adding a new shader is done entirely outside `Game1`.

---

## Adding a New Shader After This Change

To add `GlassRenderPass` (or any future pass):

1. Create `Rendering/GlassRenderPass.cs` implementing `IRenderPass`.
   - `ClaimsShape`: returns `shape is GlassPane`.
   - `BuildBuffer`: creates a `ShapeBuffer` with `BlendState.AlphaBlend` and the glass vertex buffer.
   - `Draw`: captures scene, runs blur, draws glass quads.
   - `DrawOrder`: 50 (after opaque, before skybox).
   - `LoadContent`: loads the `.fx` shader.
2. In `Game1.LoadContent`, add one line: `_renderPassRegistry.Register(new GlassRenderPass(...))`.

**No other files change.**

---

## Migration Steps

1. Add `IRenderPass` and `RenderPassRegistry`.
2. Convert `TwoPassRenderEffect` → `OpaqueRenderPass`.
3. Convert `GrassRenderEffect` → `GrassRenderPass`.
4. Convert `SkyboxRenderEffect` → `SkyboxRenderPass`.
5. Update `ShapeBufferCreator` to use the registry.
6. Update `LevelData` to use `BuffersByPass`.
7. Update `LoadedLevelData` (remove `SkyboxBuffer` special-case).
8. Update `Game1` (registry loop).
9. Delete `ShapeBufferType` enum.

Each step can be done and tested individually, with the pass registry growing one entry at a time.

---

## Considerations

- **Ordering of `ClaimsShape` checks:** Passes are checked in `DrawOrder` sequence; first match wins. `OpaqueRenderPass` should have the highest `DrawOrder` so it only claims what no specialized pass wants.
- **Test impact:** `ShaderReworkTests`, `ShaderLightCountTests`, and visual regression tests will need their buffer-access code updated (from named properties to `BuffersByPass[pass]`), but no test logic changes.
- **Skybox special-casing:** The current `LoadedLevelData.SkyboxBuffer` is a single instance across segments. After the change, `SkyboxRenderPass.Draw` should ignore duplicate skybox buffers (take only the first one), keeping the same behavior without the special property.
