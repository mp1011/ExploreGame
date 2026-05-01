# GOAL
Create a camera override that turns to face a target object. This will be used for cutscenes

# CLASS
- `Camera\LookAtCamera.cs`
- inputs and constants:
    - ICamera previous: original camera, used to get the starting view matrix
    - Shape lookAt: shape to turn towards 
    - _deltaPerFrame : determines the speed at which the camera turns 

# BEHAVIOR
- each time CreateViewMatrix is called, rotate the original matrix (_lastView) so that it turns to face the shape
- rotate only, the view matrix should maintain its position. Imagine that the player is turning their head to look at something
- the amount the matrix should change is determined by _deltaPerFrame
- once the rotational distance is below _deltaPerFrame, just set the camera to be facing the shape

# TESTING
- this will be tested manually. I will tell you if it worked.