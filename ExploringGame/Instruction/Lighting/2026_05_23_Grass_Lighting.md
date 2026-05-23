# GOAL
Apply normal-based point lighting to the grass shader

# CONTEXT
- grass shader: `ExploringGame\Content\GrassEffect.fx`
- point light shader for reference: `ExploringGame\Content\PointLightEffect.fx'

# PARAMETERS
- instead of an array of lights, the grass effect has just two intensities and positions. The intensity value will be 0 if a light is unused
    float3 LightPosition1;
    float3 LightIntensity1;
    float3 LightPosition2;
    float3 LightIntensity2;

# TASK
    - add Normal to the vertex definition of the grass shader
    - use the same lighting formula for normal-based lighting that you see in PointLightEffect.fx
    - similar to PointLightEffect, compute the light value using both lights and return the maximum
   
# DEVELOPMENT PLAN
Read it. To add this feature cleanly, the work is mainly in the grass vertex format, the grass buffer builder, and ExploringGame/Content/GrassEffect.fx.
What the feature is asking for
The note is asking for three concrete changes:
1.	The grass shader must receive a normal per vertex
2.	The grass shader must use the same normal-based point-light formula as PointLightEffect.fx
3.	It must evaluate both grass lights and use the brighter result
The good news is that the grass render path already supplies:
•	LightPosition1
•	LightIntensity1
•	LightPosition2
•	LightIntensity2
So the missing work is mostly about feeding normals into the shader and replacing the placeholder lighting logic.
---
Files involved
1. ExploringGame/Content/GrassEffect.fx
This is the main shader that must change.
Right now it:
•	accepts root position, offset, texcoord, rotation, and color
•	computes a billboarded blade orientation in the vertex shader
•	creates a blade normal procedurally
•	does not actually use real point lighting
•	still has a placeholder:
•	float normalDot = 1.0; // fix me
This is where the real lighting behavior must be implemented.
2. ExploringGame/Rendering/GrassVertex.cs
This is the CPU-side vertex structure that defines what data is sent to the grass shader.
Right now it contains:
•	RootPosition
•	Offset
•	TexCoord
•	Rotation
•	Color
It does not contain Normal, so the shader cannot legally declare one yet.
3. ExploringGame/Rendering/GrassVertexBufferBuilder.cs
This is where the grass vertices are built before going into the GPU buffer.
This file will need to populate the new normal field for every GrassVertex.
4. ExploringGame/Rendering/RenderEffects/GrassRenderEffect.cs
This already sends the two light positions and intensities to the shader.
This likely needs little or no logic change. It is already set up for the requested parameter model.
5. ExploringGame/GeometryBuilder/Shapes/GrassSurface.cs
Probably no functional lighting logic belongs here, but it is important for understanding how the grass blades are authored:
•	each blade is created as two triangles
•	the authored blade geometry lies in a simple plane before the shader billboards it
That matters because it tells you what the input normal should represent.
---
How to implement it
Step 1: Extend the grass vertex format with a normal
In GrassVertex.cs, add a Vector3 Normal field and update the vertex declaration.
That means:
•	add the field to the struct
•	update the constructor
•	add a VertexElement with usage Normal
•	shift any offsets that come after it
Why this is necessary:
•	HLSL input semantics must match the CPU vertex declaration
•	if the shader declares NORMAL0 but the vertex declaration does not provide it, the shader input layout will not match
Important detail:
•	the new vertex layout must stay tightly aligned with the struct layout
•	once the normal is added, the byte offsets for later fields will move
---
Step 2: Decide what the grass normal means
This is the most important design point.
The grass blades are not static triangles in world space. In GrassEffect.fx, the blade is billboarded toward the camera by computing:
•	a camera-facing right vector
•	a rotated version of that right vector
•	a vertical up vector
•	a blade plane from those axes
So the normal you add should represent the blade’s local authored normal, not a terrain normal.
Best interpretation
Use a local blade normal that corresponds to the blade before billboarding.
Because the authored blade is effectively built in a vertical plane, the local normal can be something like:
•	forward/back along the plane normal for the unrotated blade
Then in the shader, transform that local normal into the final billboarded world normal.
Simpler alternative
You could also ignore the stored normal when computing the final lit normal and continue deriving the final blade normal from:
•	cross(rotatedRight, up)
That would still produce the correct world-space blade normal for lighting.
However, the task explicitly says to add Normal to the vertex definition, so the clean implementation is:
•	add the normal to the vertex stream
•	use it as the local-space blade normal basis
•	rotate/rebuild it consistently in the shader
---
Step 3: Populate the normal in GrassVertexBufferBuilder.cs
When the grass buffer is built, each GrassVertex needs the new normal value.
Because each blade is a vertical strip, all vertices of the same blade can use the same local normal.
What to assign
Use a consistent local blade normal for every vertex in that blade.
For example:
•	the blade is authored as a vertical plane
•	its local normal should be perpendicular to that plane
What matters is consistency:
•	both triangles of the blade should use the same local normal direction
•	otherwise lighting can split across the blade or flip unexpectedly
Why not use triangle normals from the generated triangles
You technically could derive a geometric triangle normal from the triangle data, but that is less useful here because:
•	the authored triangles are only a temporary CPU representation
•	the shader later re-orients the blade toward the camera
•	static triangle normals would no longer match the final rendered orientation unless you also reorient them in the shader
So the better mental model is:
•	store a blade-local normal
•	rotate it along with the billboard orientation in the shader
---
Step 4: Update GrassEffect.fx input/output structures
The shader input must be updated to accept the new normal.
Vertex input
Add Normal : NORMAL0 to VSInput.
Pixel input
Right now the grass shader passes:
•	clip-space position
•	texcoord
•	scalar brightness
•	color
That is not enough for real point lighting.
To follow the point-light reference cleanly, the pixel stage should also receive:
•	world position
•	final world normal
That means the grass shader should pass something like:
•	WorldPos
•	Normal
from vertex shader to pixel shader.
Why world position is needed
The point-light formula depends on the vector from the fragment to the light:
•	lightVector = lightPosition - worldPosition
Without world position, you cannot compute distance attenuation correctly.
---
Step 5: Build the final world-space grass normal in the vertex shader
This is where the current shader already gives you most of the work for free.
Today the vertex shader computes:
•	up
•	right
•	rotatedRight
•	bladeNormal = normalize(cross(rotatedRight, up))
That bladeNormal is already basically the billboarded blade normal in world space.
So the implementation should do one of these:
Preferred
Treat the incoming vertex normal as the local blade normal, and derive the world-space normal from the billboard basis.
That keeps the pipeline conceptually correct:
•	CPU provides local normal
•	shader computes final world normal after billboarding
Practical shortcut
Use the existing billboard-derived bladeNormal as the normal that gets passed to the pixel shader.
This is likely visually correct for the current grass system because the blade plane is fully defined in the shader by rotatedRight and up.
Either way, the final normal passed to the pixel shader should be:
•	normalized
•	in world space
•	matched to the billboarded blade surface
---
Step 6: Replace the placeholder brightness logic with the point-light formula
This is the core of the feature.
In PointLightEffect.fx, the normal-based lighting formula is:
•	compute vector from surface to light
•	compute distance
•	normalize light direction
•	compute NdotL = saturate(dot(normal, lightDir))
•	compute attenuation as saturate(1 - distance / 8)
•	compute light amount as NdotL * attenuation * intensity
You should reproduce that logic in the grass shader for each of the two lights.
Important difference from the reference shader
PointLightEffect.fx uses:
•	scalar intensity values
The grass shader uses:
•	float3 LightIntensity1
•	float3 LightIntensity2
So in the grass shader, the result should be RGB light contribution, not just a scalar brightness.
That means each light contribution is effectively:
•	scalar lambert-and-attenuation term
•	multiplied by a color/intensity vector
So the output of each light should be a float3.
---
Step 7: Evaluate both lights and take the maximum
The note explicitly says:
•	compute the light value using both lights
•	return the maximum
Because the grass effect has exactly two light slots, the logic is straightforward:
•	evaluate contribution from light 1
•	evaluate contribution from light 2
•	use component-wise max of the two
This is conceptually similar to what PointLightEffect.fx does when it keeps the best normal-light ratio over all lights.
Why max instead of add
Using max:
•	avoids over-brightening from overlapping lights
•	matches the requested behavior
•	is consistent with the reference shader’s “best light wins” approach
---
Step 8: Combine that light with the existing grass appearance logic
The current grass shader has two appearance modifiers:
1.	a fake normal-light term, currently placeholder
2.	a height-based ambient occlusion factor:
•	darker near the base
•	brighter toward the tip
You likely want to keep the height-based factor because it adds depth to the blade.
So the final result should be thought of as:
•	sampled grass texture
•	multiplied by vertex color
•	multiplied by point-light result from the brighter of the two lights
•	optionally shaped by the existing base-to-tip darkening
Recommended behavior
Keep the height factor, but make it a secondary modifier rather than the main light source.
That way:
•	the feature request is satisfied by real point lighting
•	the grass still keeps its nice grounded look
What to remove
The current scalar brightness path based on:
•	normalDot = 1.0
•	normalLighting = saturate(normalDot * 0.5 + 0.5) should be replaced, because it is not real lighting
---
Step 9: Be careful about unused lights
The note says:
•	intensity is 0 if a light is unused
The render effect already follows that pattern.
So the shader should not need special branching for “light present or not.” If intensity is zero, the computed contribution naturally becomes zero.
That keeps the shader simple.
---
Step 10: Verify one subtle issue: two-sided grass
Your grass pass uses RasterizerState.CullNone, so both sides of the grass are rendered.
That introduces a subtle lighting question:
•	if you use the exact point-light formula with a single normal,
•	the back side of the blade may go dark because dot(normal, lightDir) becomes negative and clamps to zero
This is expected with strict one-sided Lambert lighting
It is also faithful to the reference formula.
If that looks wrong visually
A later refinement could make grass lighting two-sided by:
•	flipping the normal for backfaces, or
•	using the absolute value of the dot product
But that would no longer be exactly the same formula as the point-light reference.
So for this task, the best approach is:
•	implement the same formula first
•	only adjust for two-sided lighting if the visual result is unacceptable
---
What likely does not need to change
GrassRenderEffect.cs
This is already very close to what the feature needs.
It already:
•	sets world/view/projection
•	sets camera position
•	sends up to two light positions/intensities
•	zeroes intensity when no light is present
Unless you discover a mismatch in parameter naming, this file is probably fine.
GrassSurface.cs
This file creates the blade geometry, but the feature does not require changing the authored grass shape itself unless you decide to derive normals differently.
---
Practical implementation order
The safest order is:
1.	Update GrassVertex.cs
•	add Normal
•	fix vertex declaration offsets
2.	Update GrassVertexBufferBuilder.cs
•	assign a local normal for each grass vertex
3.	Update GrassEffect.fx input structs
•	accept the normal
•	pass world position and final normal to the pixel shader
4.	Implement the two-light normal-based lighting in the grass shader
•	mirror the point-light normal formula
•	use both lights
•	take max
5.	Blend with existing grass appearance
•	keep height-based darkening if desired
•	remove the placeholder fake normal-light path
6.	Test visually
•	one light
•	two lights
•	no lights
•	camera moving around the grass
•	light above vs beside the grass
---
What to look for during testing
Correct results
•	grass gets brighter when a point light is near it
•	the tip/base shaping still looks natural
•	the brighter of the two lights dominates
•	unused light slots do nothing
Common failure signs
•	grass turns black everywhere
•	normal or world position is wrong
•	lighting does not move when the light moves
•	world position being passed to pixel shader is wrong
•	lighting ignores blade orientation
•	normal is not being transformed correctly after billboarding
•	shader compiles but renders garbage
•	vertex declaration no longer matches shader input order/offsets
•	only one side of blades lights correctly
•	expected with one-sided Lambert; decide whether that visual is acceptable
---
Recommended mental model
The cleanest way to think about this feature is:
•	GrassVertex should now describe a blade vertex in local terms
•	the vertex shader should turn that into a billboarded world-space blade
•	the pixel shader should apply real point lighting using:
•	world position
•	world normal
•	two provided lights
•	max of the two contributions
That matches both the instruction note and the way the existing render path is already structured.
If wanted, I can next turn this into a concrete file-by-file implementation checklist without writing the code yet.