# SUMMARY
The existing tiling system is unreliable, often resulting in incorrectly drawn textures

## OLD APPROACH
- each individual shape would split its triangles if using a tiled texture
- this split assumed the triangles were axis aligned
- the triangles would be split such that any single triangle would only have one occurence of the texture

## FLAWS OF THE OLD APPROACH
- often there are multiple shapes next to eachother with the same texture, and you'd expect the texture to tile seamlessly across them.
    - however since each shape is split independently, it can produce odd gaps
- not all surfaces are axis aligned

## Phase 1
- in SplitTrianglesIfNeeded:
    - call sideTriangles.GetCornerVertices(side) to get two vertices that describe an infinite 2D plane.
    - project the zero vector onto this plane. This will be our texture origin.
        - this differs from the existing code which used the bounding box corner of the shape as the texture origin.
        - now, we will use the same texture origin for shapes that draw to the same plane, which should remove seams between neighboring shapes
- SplitTriangleIntoTiles should work as follows
    - project the triangles onto the plane formed by the two corner vertices, to bring us down to 2 dimensions
    - in this projection, a UV of 0,0 represents the texture origin.
    - create a "grid" of vertices, each textureInfo.TileSize apart
    - whenever one of these grid vertices intersects a triangle, split the triangle into several such that this new vertex is included
        - TriangleSubdivider should be doing this already
        - these new vertices represent points where the texture would be 0,0 or 1,1
    - unproject the triangles back to 3D
 - in VertexBufferBuilder -> CalcTextureCoordinates_Tile
     - we need to use the SAME plane and texture origin calculation that we used in SplitTriangleIntoTiles
    
## TESTING
- do not run automated tests until I tell you
- I will verify through manual testing and tell you what worked or not

## QUESTIONS
1.	How should the plane/texture origin be shared? Should this calculation happen once and be stored somewhere accessible to both SplitTriangleIntoTiles(Triangle, Vector2, Vector3) and CalcTextureCoordinates_Tile(Side, TextureSheet, Triangle, Vector3, (Vector3, Vector3)), or should both methods independently recalculate it (ensuring they use identical logic)?
    - use the same code in both places. (don't copy paste, create helper functions if needed)
2.	What does "project the zero vector onto this plane" mean precisely? Is this finding the closest point on the plane to the world origin (0,0,0)?
    - the goal here is to find a consistent point to treat as the texture origin, regardless of where the shape is. You may implement as you see fit.
3.	The 2D projection: When you say "project the triangles onto the plane formed by the two corner vertices," are we establishing a local 2D coordinate system on the plane? What should be the basis vectors for this 2D space?
    - yes you are establishing a local 2d coordinate system. Which axis is U or V doesn't matter as long as we're consistent