# GOAL
Improve how we calculate texture coordinates for shapes that aren't AABB, so we can properly overlay the sky texture on the skydome

# FILES TO READ
- GeometryBuilder\Shapes\Skyboxes\SkyDome.cs
- Rendering\VertexBufferBuilder.cs

# TESTING
- Program.cs is currently set up for manual testing. I will tell you if the texture looks right.

# RESEARCH
- look into how we assign texture coordinates. Note that the existing code assumes shapes are AABB. For our dome shape, this isn't the case.

## Research Findings

### Current Implementation (AABB-Based)

The texture coordinate system in `VertexBufferBuilder.cs` is designed for **Axis-Aligned Bounding Box (AABB)** shapes:

1. **Process Flow** (`CreateVertices` method):
   - Groups triangles by Side (North, South, East, West, Top, Bottom)
   - Calls `GetCornerVertices()` to find bounding box corners for each side
   - Calculates texture coordinates using `CalcTextureCoordinates()` based on TextureStyle

2. **GetCornerVertices** (`VectorExtensions.cs`):
   - Finds the axis-aligned bounding box of all vertices for a given side
   - Returns two corner vertices that should map to UV (0,0) and (1,1)
   - **Assumption: Vertices fit in a rectangular bounding box**

3. **CalcTextureCoordinates_FillSide** (used by SkyDome's TextureStyle):
   - Projects 3D vertex to 2D using `As2D(side)` - drops one axis based on the side
   - Calculates relative position: `(vertex - topLeft) / (bottomRight - topLeft)`
   - **Result: Linear interpolation within bounding rectangle**

### Why This Fails for SkyDome

**SkyDome Geometry** (`TriangleMaker.BuildEllipsoid`):
- Generated using **spherical coordinates** (latitude θ and longitude φ)
- Creates vertices at: `(rx*cos(φ)*sin(θ), ry*cos(θ), rz*sin(φ)*sin(θ))`
- Shape is **curved**, not axis-aligned
- All triangles assigned to Side.Top and Side.North (no meaningful side separation for spherical geometry)

**Problems with AABB Projection**:
1. **Flattening distortion**: `As2D()` drops an axis (e.g., X,Z for North side), losing 3D curvature information
2. **Pole clustering**: Top vertices (near pole) cluster in 3D but should spread across texture U coordinates
3. **Horizon stretching**: Bottom vertices (horizon) are far apart but get compressed in bounding box projection
4. **Lost angular information**: The longitude angle (0-360°) wrapping is not captured by linear interpolation
5. **Non-linear relationship**: Distance in 3D space ≠ distance in UV space for curved surfaces

**Example**: Two dome vertices at the same height but 180° apart in longitude will project to similar Y coordinates but should have U coordinates of ~0 and ~0.5.

### Correct Approach: Spherical UV Mapping

For dome/sphere shapes, use **spherical (polar) coordinate mapping**:

```
U = atan2(z - center.Z, x - center.X) / (2π) + 0.5    // Longitude: 0° to 360° → 0 to 1
V = (y - yMin) / (yMax - yMin)                        // Height: bottom to top → 0 to 1
```

For ellipsoids (stretched spheres):
```
// Normalize to unit sphere first
nx = (x - center.X) / rx
ny = (y - center.Y) / ry
nz = (z - center.Z) / rz

U = atan2(nz, nx) / (2π) + 0.5
V = asin(ny) / π + 0.5
```

This preserves the angular relationships from the spherical generation method.

### Recommended Solution

**Option 1**: Add `TextureStyle.Spherical` enum value
- Add new calculation method `CalcTextureCoordinates_Spherical` to VertexBufferBuilder
- Store Shape dimensions (Width, Height, Depth) and Position in Triangle or pass Shape to CalcTextureCoordinates
- Update SkyTheme to use `TextureStyle.Spherical`

**Option 2**: Special case detection
- Detect when a shape is spherical/dome-like in `CreateVertices` 
- Bypass the standard side-based processing for spherical shapes
- Apply spherical mapping directly

**Option 1 is cleaner** - it extends the existing TextureStyle system and can be reused for other spherical shapes (planets, balls, etc.).

---

# TECHNICAL PLAN

### Prerequisites
- Program.cs is set up for manual testing (true)
- Sky texture is loaded via TextureSheetKey.Sky (true)
- SkyDome is visible in the scene (true)

### Step 1: Add Spherical TextureStyle
**File**: `ExploringGame\Texture\TextureInfo.cs`
- Add `Spherical` to the `TextureStyle` enum
- No changes needed to TextureStyleExtensions (spherical doesn't tile)

**Expected**: Code compiles with new enum value available

### Step 2: Store Shape Metadata in Triangle
**File**: `ExploringGame\GeometryBuilder\Basic.cs`
- Add Shape reference or shape dimensions (Position, Width, Height, Depth) to Triangle class
- This data is needed for spherical coordinate calculations

**Alternative**: Pass Shape to CalcTextureCoordinates (requires signature changes in VertexBufferBuilder)

**Expected**: Triangle can provide shape center and radii to UV calculation

### Step 3: Implement Spherical UV Calculation
**File**: `ExploringGame\Rendering\VertexBufferBuilder.cs`
- Add `CalcTextureCoordinates_Spherical` method
- Implementation:
  ```csharp
  // Extract shape center and radii from triangle
  // For dome/ellipsoid:
  // U = atan2((z - center.Z) / rz, (x - center.X) / rx) / (2π) + 0.5
  // V = (y - (center.Y - ry)) / (2 * ry)  // Maps bottom to top of dome
  ```
- Add case to switch statement in `CalcTextureCoordinates()`

**Expected**: New method compiles and is reachable via TextureStyle.Spherical

### Step 4: Update SkyTheme
**File**: `ExploringGame\Texture\Theme.cs`
- Change `SkyTheme` MainTexture from `TextureStyle.FillSide` to `TextureStyle.Spherical`

**Expected**: SkyDome now uses spherical UV mapping

### Step 5: Initial Visual Test
**Action**: Run Program.cs and observe the sky dome
**Look for**:
- Texture should wrap smoothly around the dome
- No stretching or compression at poles
- Horizon line appears level and continuous
- No seams or discontinuities (except at longitude 0°/360° boundary)

**Common Issues**:
- **Seam visible**: Check if U wrapping needs adjustment (may need 0-1 vs 0.5-1.5 offset)
- **Upside down**: V coordinate may need to be inverted (1 - V)
- **Rotated**: U coordinate offset may need adjustment (+0.5, +0.25, etc.)
- **Stretched at poles**: Verify normalization by ellipsoid radii is correct

### Step 6: Fine-Tune UV Mapping
**Potential adjustments**:
- **U offset**: Rotate texture horizontally by adjusting the atan2 result
- **V direction**: Flip V if texture appears upside down
- **V range for dome**: Since dome is only top hemisphere, may need to map to V range 0.5-1.0 instead of 0-1

**Test cases**:
1. Sky texture with visible features (clouds, horizon gradient)
2. Rotate camera 360° - horizon should be continuous
3. Look straight up - pole should show minimal distortion
4. Check seam at longitude 0° (often behind player start position)

### Step 7: Verify No Regression
**Action**: Check that other shapes still render correctly
**Test shapes**:
- Rooms (boxes) with TextureStyle.FillSide
- Floors/walls with TextureStyle.Tile
- Hallways with TextureStyle.HorizontalRepeat

**Expected**: No changes to non-spherical shapes

### Success Criteria
- [ ] Sky texture maps naturally to dome curvature
- [ ] No visible stretching or compression
- [ ] Continuous horizon line (no breaks)
- [ ] Minimal seam visibility at longitude boundary
- [ ] Existing AABB shapes unaffected