# Glass Windows Implementation Plan

## Overview

Add glass panes to existing `Window` shapes that render a slight blur of the scene behind them, giving windows a frosted/glass appearance.

---

## Architecture

Glass blur in MonoGame requires a render-to-texture approach:

1. **Scene pre-pass** – render the full world to an off-screen `RenderTarget2D` before drawing the glass geometry.
2. **Blur pass** – downsample that render target and apply a Gaussian blur shader (one horizontal, one vertical pass) into a second `RenderTarget2D`.
3. **Glass pass** – render the glass quad geometry using the blurred texture as input, blending it with a tinted/translucent overlay.

---

## New Files

### `Rendering/GlassRenderEffect.cs`
New class (similar to `GrassRenderEffect`) responsible for the glass draw pass.

- Holds references to the two `RenderTarget2D` objects (scene capture + blurred result).
- Exposes a `CaptureScene(GraphicsDevice, Action drawScene)` method called before the normal world draw, which renders the scene into the capture target.
- `Draw(GraphicsDevice, IEnumerable<ShapeBuffer>, Matrix view, Matrix projection)` draws only the glass-tagged `ShapeBuffer`s using the blurred texture.
- Loads and owns the `GlassEffect.fx` shader.

### `Content/GlassEffect.fx`
HLSL shader with two techniques:

- **BlurH** – horizontal Gaussian blur; samples the scene render target with a 9-tap kernel along U.
- **BlurV** – vertical Gaussian blur; samples the blurred horizontal result along V.
- **Glass** – final pass; samples the fully-blurred texture at the screen-space UV of the glass quad and blends it with a configurable tint color and opacity.

Shader parameters:
```hlsl
Texture2D SceneTexture;   // scene render target (for blur passes)
Texture2D BlurTexture;    // blurred result (for glass pass)
float2 TextureSize;       // viewport resolution for UV offset calculation
float4 TintColor;         // glass tint (default: slightly blue/white, alpha ~0.15)
float  BlurStrength;      // controls blur kernel radius (default: 1.0)
```

### `GeometryBuilder/Shapes/Structures/GlassPane.cs`
A thin `Box`-derived shape that represents a single glass pane inside a `Window`.

- Created by `Window` constructor (or a new `Window` constructor overload `withGlass: bool`).
- Sets `OmitSides` to show only the two large faces (front and back of the pane).
- Assigns a dedicated `TextureInfo` using a new `TextureKey.Glass` so `ShapeBufferCreator` can tag its `ShapeBuffer` as `ShapeBufferType.Glass`.

---

## Changes to Existing Files

### `Rendering/ShapeBuffer.cs`
Add a new `ShapeBufferType` value:
```csharp
public enum ShapeBufferType { Normal, Grass, Glass }
```

### `Texture/TextureSheet.cs` / `TextureInfo.cs`
Add `TextureKey.Glass` – a plain white 1×1 texture used as a placeholder since the glass shader does not sample a texture atlas.

### `Services/ShapeBufferCreator.cs`
In `CreateShapeBuffers`, detect `GlassPane` shapes and create their `ShapeBuffer` with `Type = ShapeBufferType.Glass` and `BlendState = BlendState.AlphaBlend`.

### `LevelControl/LoadedLevelData.cs`
Add `IEnumerable<ShapeBuffer> GlassShapeBuffers` property (analogous to `GrassShapeBuffers`) so `Game1` can pass glass buffers to `GlassRenderEffect`.

### `GeometryBuilder/Shapes/Structures/WindowJunction.cs`
In the `Window` constructor, after building the sill/curtains, optionally instantiate a `GlassPane` child:
```csharp
if (withGlass)
    new GlassPane(this, _wallSide);
```

Add `withGlass = false` default parameter to keep all existing call sites unchanged.

### `Game1.cs`
In `LoadContent`:
- Instantiate `GlassRenderEffect` and call `SetTextures`.

In `DrawWorld`:
1. Before drawing anything, call `GlassRenderEffect.CaptureScene(graphicsDevice, () => { /* existing draw calls */ })`.
2. After the existing draw calls, draw glass: `_glassRenderEffect.Draw(graphicsDevice, levelData.GlassShapeBuffers, ...)`.

---

## Rendering Order

```
1. Scene captured to RenderTarget2D (full opaque geometry)
2. Horizontal blur pass → BlurH RenderTarget2D
3. Vertical blur pass   → BlurV RenderTarget2D (= final blur)
4. Clear back buffer; draw opaque geometry normally
5. Draw glass quads (alpha-blended, sampling BlurV texture via screen-space UV)
6. Skybox
7. HUD / SpriteBatch
```

> **Important:** Because glass uses screen-space UVs derived from clip-space position, the glass shader must reconstruct the screen UV in the pixel shader:
> `float2 screenUV = input.ScreenPos.xy / input.ScreenPos.w * 0.5 + 0.5;`

---

## Rooms / Level Integration

In any `Room` class that uses `Window`, pass `withGlass: true` to add glass panes:

```csharp
new Window(this, Side.North, width: Measure.Feet(3), height: Measure.Feet(4), withGlass: true);
```

No other room-level changes are needed.

---

## Testing

- Add a `VisualRegressionTests/GlassWindowVisualTests.cs` (analogous to `LightingVisualTests.cs`) that captures a screenshot of a room with a glass window and compares against a baseline.
- Manually verify: standing in front of a window should show a blurred version of the exterior scene through the glass.

---

## Potential Issues / Notes

- **Performance:** Two extra full-screen blur passes per frame. To mitigate, render the capture target at half resolution (`viewport / 2`).
- **Multiple glass panes:** If several windows are visible simultaneously they all share the same blurred scene capture, which is acceptable.
- **Depth sorting:** Glass quads must be rendered after all opaque geometry with `DepthBufferWriteEnable = false` to avoid occluding objects behind them.
- **Existing windows:** All existing `Window` usages default to `withGlass: false`, so behaviour is unchanged until explicitly opted in.
