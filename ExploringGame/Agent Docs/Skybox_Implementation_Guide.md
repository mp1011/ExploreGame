# Skybox Implementation Guide

## Overview

The skybox system allows WorldSegments to display distant backgrounds that rotate with the camera but don't translate with player movement, creating the illusion of infinite distance. Common uses include outdoor skies, distant mountains, or space backgrounds.

## Architecture

### Key Components

1. **SkyboxShape** (`GeometryBuilder/Shapes/SkyboxShape.cs`)
   - Abstract base class for all skyboxes
   - Overrides `GetWorldMatrix()` to return `Matrix.Identity` (no translation)
   - Fixed at origin (`Vector3.Zero`) with size 50x50x50
   - Always viewed from inside (`ViewFrom.Inside`)

2. **Rotation-Only View Matrix** (`Services/CameraService.cs`)
   - `SkyboxView` property provides view matrix with translation stripped
   - `CreateRotationOnlyView()` zeros M41, M42, M43 components
   - Skybox rotates with camera but stays centered on player

3. **DepthStencilState** (`Rendering/ShapeBuffer.cs`, `Services/ShapeBufferCreator.cs`)
   - Skybox reads but doesn't write to depth buffer
   - Renders last, only filling pixels not covered by geometry
   - Configuration:
     ```csharp
     DepthBufferEnable = true
     DepthBufferWriteEnable = false
     DepthBufferFunction = CompareFunction.LessEqual
     ```

4. **Buffer Management** (`LevelControl/LoadedLevelData.cs`)
   - `SkyboxBuffer` property stores the skybox ShapeBuffer
   - Built once when first segment with skybox loads
   - Cached and reused across all segments

5. **Rendering** (`Game1.cs`)
   - Skybox renders after all geometry
   - Uses `SkyboxView` instead of regular `View` matrix
   - RenderEffect automatically handles DepthStencilState

## How to Create a New Skybox

### Step 1: Create Skybox Class

Create a class that extends `SkyboxShape`:

```csharp
using ExploringGame.GeometryBuilder;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Skyboxes;

public class MySkybox : SkyboxShape
{
    private static MySkybox _instance;
    public static MySkybox Instance => _instance ??= new MySkybox();

    public override Theme Theme { get; }

    private MySkybox()
    {
        // Configure theme and textures
        Theme = new Theme(TextureSheetKey.YourTextureSheet);
        
        // Set colors/textures for each face
        Theme.SideTextures[Side.Top] = new TextureInfo(Color.SkyBlue);
        Theme.SideTextures[Side.North] = new TextureInfo(Color.LightGreen);
        // ... configure other sides
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        // Use BuildCuboid for simple cube skybox
        return TriangleMaker.BuildCuboid(this);
        
        // OR build custom geometry for more complex skyboxes
    }
}
```

### Step 2: Apply to WorldSegment

Override the `Skybox` property in your WorldSegment:

```csharp
public class MyOutdoorSegment : WorldSegment
{
    public override SkyboxShape Skybox => MySkybox.Instance;
    
    // ... rest of segment implementation
}
```

That's it! The system handles everything else automatically:
- Skybox geometry builds when segment loads
- Buffer created with proper depth state
- Renders with rotation-only view matrix
- Cached for performance

## Technical Details

### Rendering Pipeline

1. **World Matrix**: Skybox returns `Matrix.Identity` (stays at origin)
2. **View Matrix**: Uses `SkyboxView` (rotation only, no translation)
3. **Result**: Skybox rotates with camera but doesn't move with player

### Depth Buffer Strategy

- Skybox renders **last** (after all geometry)
- **DepthBufferWriteEnable = false**: Doesn't affect depth buffer
- **CompareFunction.LessEqual**: Only draws where no geometry exists
- **Performance**: GPU skips skybox pixels covered by geometry

### Singleton Pattern

Skyboxes use static `Instance` properties because:
- Geometry is identical across all uses
- Saves memory (one skybox shared by multiple segments)
- Ensures consistent appearance

### Multiple Segments

When multiple segments have skyboxes:
- **First loaded wins**: First non-null skybox is built and cached
- Subsequent segments reuse the same skybox
- To change skybox: Clear `LoadedLevelData.SkyboxBuffer` and load new segment

## Example: TestSkybox

The `TestSkybox` demonstrates the pattern:
- **Top**: Sky blue (RGB 135, 206, 235)
- **North**: Pink (RGB 255, 182, 193)
- **South**: Light green (RGB 144, 238, 144)
- **East**: Light yellow (RGB 255, 255, 224)
- **West**: Light cyan (RGB 224, 255, 255)
- **Bottom**: Dark gray (RGB 64, 64, 64) - not visible when standing on floor

## Advanced Techniques

### Custom Geometry

Instead of `BuildCuboid()`, you can create custom shapes:

```csharp
protected override Triangle[] BuildInternal(QualityLevel quality)
{
    var triangles = new List<Triangle>();
    
    // Create sphere, dome, or custom geometry
    // Use TriangleMaker helper methods or manual triangle creation
    
    return triangles.ToArray();
}
```

### Texture Mapping

Use actual textures instead of solid colors:

```csharp
Theme.SideTextures[Side.Top] = new TextureInfo(
    textureLocation: new Vector2(x, y),
    textureSize: new Vector2(width, height)
);
```

### Lighting Control

Skyboxes have `LightingGroup = null` (self-lit by default). To adjust brightness, modify colors in the Theme or override lighting properties.

## Key Files

- **Base Class**: `GeometryBuilder/Shapes/SkyboxShape.cs`
- **Example**: `GeometryBuilder/Shapes/Skyboxes/TestSkybox.cs`
- **Integration**: `GeometryBuilder/Shapes/WorldSegments/WorldSegment.cs`
- **View Matrix**: `Services/CameraService.cs`
- **Buffer Creation**: `Services/ShapeBufferCreator.cs`
- **Buffer Storage**: `LevelControl/LoadedLevelData.cs`
- **Rendering**: `Game1.cs`, `Rendering/RenderEffect.cs`, `Rendering/ShapeBuffer.cs`

## Testing

Test map available: `TestMaps.SkyboxTest()`
- Single room with floor only
- TestSkybox visible on all sides
- Validates rotation without translation behavior

## Common Issues

**Skybox not visible?**
- Ensure WorldSegment.Skybox returns non-null instance
- Check that segment is loaded/active
- Verify Theme has TextureSheetKey that's loaded in LoadContent

**Skybox moves with player?**
- Verify SkyboxShape.GetWorldMatrix() returns Matrix.Identity
- Check Game1.DrawWorld uses _cameraService.SkyboxView (not View)

**Skybox occludes geometry?**
- Verify ShapeBufferCreator.CreateSkyboxBuffer() sets DepthBufferWriteEnable = false
- Check skybox renders after geometry in Game1.DrawWorld

**Different skybox needed per segment?**
- Currently uses first loaded skybox
- To implement per-segment skyboxes, modify LoadedLevelData.LoadSegment to check active segment instead of null check
