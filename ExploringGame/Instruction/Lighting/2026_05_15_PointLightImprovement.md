# GOAL
## CURRENT STATE 
- Point Lights only apply to their own Lighting Group
## DESIRED STATE
- Point Lights apply to their own Lighting Group and up to N neighbors
    - start with N=2

# CONTEXT
- `Agent Docs\project_map.md`
- `Rendering\RenderEffect.cs` - contains the point light render effect
- `Services\RoomLightingCalculator.cs` - connected graph of light sources

# INSTRUCTION
1. Start by researching the codebase to learn about Lighting Groups and Point Lights. Write your findings into the "RESEARCH NOTES" section.
2. Write a test that turns off all lights except one, and asserts a neighboring room gets some of the light. Expect this test to fail.
    - please note that being a monogame project, tests CANNOT run in parallel
3. 

# RESEARCH NOTES

