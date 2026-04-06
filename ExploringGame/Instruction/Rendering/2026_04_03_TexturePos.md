# OVERVIEW
Improve how texture coordinates are calculated to better support surfaces that are not axis aligned.

# CONTEXT
- when we calculate texture coordinates, we take all triangles on one "side" (axis aligned) of the shape
- on this side, we calculate the corner vertices of the plane using Extensions\VectorExtensions.cs -> GetCornerVertices
- this is a simple minimum/maximum which gets an axis-aligned plane
- these points are used for texture coordinates, with the minimum being used for texture 0,0 and the max being used for texture 1,1
- however, this breaks down completely when the surface is NOT axis aligned

# QUESTIONS
1. Can we still use "Side" to compute the plane?
    - yes but not in the same way as before
    - side alone is no longer entirely sufficient to determine the plane, as it was when we assumed everything was axis aligned
    - however, all triangles on the same side WILL be near to the same plane, its just that plane may be skewed
2. What kind of shapes are we considering for this enhancement?
    - while the initial coding assumed axis aligned boxes, let this new system assume there are still "sides" to a polygon
    - and that each side is ROUGHLY on the same plane 

# DESIRED APPROACH
1. When rendering a triangle, we group triangles by their Side
2. We compute a "best fit" plane that minimizes the distance between each vertex
3. The plane is then shrunk to the smallest rectangular segment that best encompasses all points
4. The triangles are projected onto this plane in order to get a relative positioning
5. Textures are computed based on this projection, from 0,0 at one corner of the plane to 1,1 at the other

# IMPLEMENTATION QUESTIONS
1. Best Fit Plane Calculation: Should we use Principal Component Analysis (PCA) or a simpler approach like averaging the triangle normals?
    - i have no opinion on what algorithm is best. 
    - whatever approach you choose, create a dedicated service class to compute the best fit plane so the changes are in one file
2. Plane Shrinking/Bounding
When you say "shrink to the smallest rectangular segment":
•	Do you mean an oriented bounding box (OBB) aligned with the plane's local coordinate system?
•	Or a minimum area rectangle that best fits the projected points?
    - yes an oriented bounding box. The goal is to decide what vertices should get texture coordinates 0,0 and which should get 1,1
3. Projection Math
•	Once we have the plane, we need to establish a 2D coordinate system on it. Should we:
•	Use the plane's normal to define "up"?
•	Derive two perpendicular basis vectors from the vertices themselves?
•	Align one axis with the original Side's expected orientation (e.g., for Side.North, align one axis with world X)?
    - i leave the mathematics up to you. using the planes normal as "up" sounds right
4. Backwards Compatibility
•	Should axis-aligned surfaces use the old fast path (simple min/max)?
•	Or always use the new plane-fitting approach for consistency?
•	How do we detect if a surface is "close enough" to axis-aligned?
    - no need to make a special case. the oriented bounding box will work for axis aligned anyway. 
    - even if its more expensive, the game is still simple enough that it won't be an issue
5. Edge Cases
•	What if all vertices are colinear (degenerate plane)?
•	What if there's only one triangle on a side?
•	Should we handle non-planar sides (vertices that don't lie on the same plane)?
    - for now just assume all vertices are close enough to the plane. 
    - any outliers will still get 2d projected onto the plane, they just might be distorted which is okay
6. Where Does This Fit?
•	Should this replace GetCornerVertices(this IEnumerable<Triangle>, Side) in VectorExtensions.cs?
•	Or create a new method and update callers to use it?
•	Does this affect SplitTrianglesForTiling and/or texture coordinate calculation in the rendering pipeline?
    - replace GetCornerVertices which keeping the method signature intact. We're basically just changing the implementation details
    - it likely also impacts SplitTrianglesForTiling as that requires corner vertices to decide how to build the grid
     
