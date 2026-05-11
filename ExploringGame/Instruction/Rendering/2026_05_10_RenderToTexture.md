# GOAL
- Render the scene to a RenderTarget2D so we can apply effects
- add support for changing the brightness and color of the entire scene

# TECHNICAL TASKS

1. In RenderPassRegistry, split "Passes" into "EnvironmentPasses" and "InterfacePasses"
    - add a "RegisterInterface" method to add to InterfacePasses, and use this for DialogueRenderPass
2. Adjust the rendering code in Game1.cs so that all "EnvironmentPasses" are rendered to a render target
3. Provide a pluggable mechanism for transforming the image of the render target before we draw it to screen
    - brightness
    - color
    - blur effect
    - other kinds of distortion
4. Draw the modified render target to the screen, then draw any InterfacePasses on top of it


# QUESTIONS FROM COPILOT

- Should the render-to-texture system support a **chain of transforms** (brightness + tint + blur together), or should it assume only **one active full-screen transform** at a time?
    - support chains of trnasformers
- Do you want **brightness and color** fully implemented in this task, with blur/distortion only represented by extension points, or should at least one additional non-trivial effect also ship now?
    - start with only brightness and color
- Where should the active scene-adjustment settings live: a dedicated rendering service, the active `Scene`, or debug-editable globals for now?
    - dedicated rendering service, specifically for adjusting the render target's image before it is displayed
- Should **all non-UI rendering** go through the environment render target, including skybox, opaque, glass, and grass passes, with `DialogueRenderPass` being the first `InterfacePass`?
    - correct, all other rendering goes through the environment render target
- `Game1.Draw()` currently renders debug text after `DrawWorld()` using its own `SpriteBatch`. Should that overlay stay **outside** the new interface-pass system and remain unaffected by scene post-processing?
    - yes, leave debugtext as its own separate thing

# RESEARCH

## Current rendering flow

- `Game1.LoadContent()` constructs a single `RenderPassRegistry`, creates the concrete passes, loads their content, and registers them in this order: opaque, glass, grass, skybox, dialogue.
- `LoadedLevelData.SetRenderPassRegistry()` gives that registry to `LevelData`, and `LevelData.PopulateBuffers()` groups built `ShapeBuffer`s by matching each buffer's `ShapeBufferType` to the first registered pass with the same type.
- `Game1.Draw()` currently calls `DrawWorld(GraphicsDevice)`, then does a separate `_spriteBatch.Begin()` / `_spriteBatch.End()` for debug text. That debug overlay is already outside the render-pass system.

## How pass ordering actually works

- Despite comments in `Game1.cs` mentioning draw-order numbers, `RenderPassRegistry` does **not** use a `DrawOrder` property.
- `RenderPassRegistry.Passes` sorts by `pass.ShapeBufferType`.
- `ShapeBufferType` is currently:
  - `Normal`
  - `Grass`
  - `Skybox`
  - `Glass`
  - `UI`
- So the effective frame order today is **opaque -> grass -> skybox -> glass -> dialogue**, regardless of registration comments.
- This matters for the new split into `EnvironmentPasses` and `InterfacePasses`: the existing code already treats UI as special by giving `DialogueRenderPass` `ShapeBufferType.UI`, but there is no separate collection for interface passes yet.

## What each pass is doing now

- `OpaqueRenderPass` uses `TwoPassRenderEffect`:
  - first pass: `BasicRenderEffect` for textured geometry + ambient room lighting
  - second pass: `PointLightRenderEffect` for additive point lights
- `GlassRenderPass` uses `GlassRenderEffect`, which enables alpha blending and reads depth without writing depth.
- `GrassRenderPass` uses its own shader path and draws buffers one at a time.
- `SkyboxRenderPass` uses `SkyboxRenderEffect` and a rotation-only camera view from `CameraService.SkyboxView`.
- `DialogueRenderPass` is not driven by `ShapeBuffer`s at all in practice; `Game1.DrawWorld()` detects it explicitly and calls `pass.Draw(..., null, ...)`.

## Current state management and GPU state behavior

- `RenderEffect<T>` is the common low-level helper for effect-driven passes. It:
  - stores one effect instance per loaded texture sheet
  - sets world/view/projection per `ShapeBuffer`
  - applies optional per-buffer `RasterizerState` and `DepthStencilState`
- `ShapeBuffer` already carries the render state needed for special cases (`RasterizerState`, `DepthStencilState`, `LightingGroup`, `ShapeBufferType`).
- `ShapeBufferCreator` is responsible for assigning those buffer types:
  - normal geometry defaults to `ShapeBufferType.Normal`
  - grass buffers are created as `ShapeBufferType.Grass`
  - glass buffers are created as `ShapeBufferType.Glass`
  - skybox buffers are created as `ShapeBufferType.Skybox`
- This means the pass split should be able to reuse the existing `ShapeBufferType` tagging rather than inventing a second classification system for individual buffers.

## Existing 2D rendering / post-processing hooks

- There is currently **no** use of `RenderTarget2D` anywhere in the codebase.
- There is currently **no** `GraphicsDevice.SetRenderTarget(...)` call anywhere in the codebase.
- The only `SpriteBatch` usage is:
  - debug text in `Game1.Draw()`
  - dialogue text in `DialogueRenderPass`
- That means there is no existing full-screen presentation step yet. Right now 3D passes render directly to the back buffer, and 2D overlays are drawn afterward.

## Implications for this task

- The new render-to-texture feature will need to introduce the project's first explicit **scene presentation step**:
  1. bind a `RenderTarget2D`
  2. render all environment passes into it
  3. unbind the target
  4. draw the resulting texture to the back buffer, applying the transform chain
  5. draw interface passes on top
- Since debug text is already drawn after `DrawWorld()`, leaving it outside the interface-pass system will fit naturally with the current structure.
- `Scene` and `SceneManager` are currently very lightweight and do not hold any rendering configuration. A dedicated rendering service is a better fit for the scene-adjustment settings described above.
- `DialogueRenderPass` is already effectively treated as special-case UI, so moving it to an explicit `InterfacePasses` list should align with the code's current behavior rather than fighting it.