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

### Step 1: Add Spherical TextureStyle ✓ COMPLETE
**File**: `ExploringGame\Texture\TextureInfo.cs`
- ✓ Added `Spherical` to the `TextureStyle` enum
- ✓ No changes needed to TextureStyleExtensions (spherical doesn't tile)

**Result**: Code compiles with new enum value available

### Step 2: Pass Shape Through Method Chain ✓ COMPLETE (ALTERNATIVE APPROACH)
**Decision**: Instead of storing metadata in Triangle, pass Shape through the method signatures
**Files**: `ExploringGame\Rendering\VertexBufferBuilder.cs`
- ✓ Updated `BuildBuffers` to pass `shape` to `CreateVertices` calls
- ✓ Updated `CreateVertices` signature to accept `Shape shape` parameter
- ✓ Updated `CalcTextureCoordinates` signature to accept `Shape shape` parameter
- ✓ Shape context flows through rendering pipeline only where needed

**Result**: Triangle maintains Single Responsibility, Shape context available for UV calculation

### Step 3: Implement Spherical UV Calculation ✓ COMPLETE
**File**: `ExploringGame\Rendering\VertexBufferBuilder.cs`
- ✓ Added `CalcTextureCoordinates_Spherical(Shape shape, Vector3 position)` method
- ✓ Implementation uses spherical coordinate mapping:
  ```csharp
  nx = (x - center.X) / rx
  nz = (z - center.Z) / rz
  U = atan2(nz, nx) / (2π) + 0.5
  V = (y + ry - center.Y) / (2 * ry)
  ```
- ✓ Added case to switch statement in `CalcTextureCoordinates()`

**Result**: Spherical UV mapping implemented and reachable via TextureStyle.Spherical

### Step 4: Update SkyTheme ✓ COMPLETE
**File**: `ExploringGame\Texture\Theme.cs`
- ✓ Changed `SkyTheme` MainTexture from `TextureStyle.FillSide` to `TextureStyle.Spherical`

**Result**: SkyDome now uses spherical UV mapping. Build successful.

### Step 5: Initial Visual Test ⏸️ AWAITING USER FEEDBACK
**Action**: Run Program.cs and observe the sky dome

**First Test Result**: Texture mostly correct but flipped along Y axis
**Fix Applied**: Inverted V coordinate (changed `v` to `1f - v`) to account for inside view
**Reason**: When viewing from inside a dome, texture orientation needs to be flipped vertically

**Second Test Result**: Better orientation but most of texture not visible
**Issue Identified**: Formula `(dy + ry) / (2f * ry)` maps full ellipsoid (-ry to +ry), but dome only uses top half (0 to +ry)
**Fix Applied**: Changed to `dy / ry` to map full texture across dome's actual height range
- Bottom of dome (y = center.Y): v = 0 → 1-v = 1.0 → bottom of texture
- Top of dome (y = center.Y + ry): v = 1 → 1-v = 0 → top of texture

**Awaiting Third Test**: Please restart/hot reload and verify full texture is now visible across the dome.
### Step 5: Initial Visual Test ✓ COMPLETE
**Action**: Run Program.cs and observe the sky dome

**Test History**:
1. **Initial test**: Texture flipped on Y axis → Fixed by inverting V coordinate (1 - v)
2. **Second test**: Most texture not visible → Fixed by changing V formula to `dy / ry` (dome-specific range)
3. **Third test**: Severe seam with stretched texture arc → Fixed by implementing `FixSphericalSeam` method
   - Detects triangles with U span > 0.5 (seam crossers)
   - Adjusts low U values (+1.0) to fix interpolation direction

**Final Result**: ✅ SUCCESS! Texture looks perfect
- Texture wraps smoothly around the dome
- No stretching or compression
- Proper orientation (top to bottom)
- Seam handled correctly

### Step 7: Verify No Regression ⏸️ AWAITING USER FEEDBACK
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