# STORY STRUCTURE OVERVIEW

The game's story uses a movie/play analogy and breaks its story into the following heirarchy:

Scene -> Act -> Plot Point

Scene: 
	- a major level of the game
	- has a specific WorldSegmentGroup
	- if we ever implement loading screens, they would be between Scenes

Act:
	- a gameplay segment
	- no real intrinsic meaning beyond the Plot Points it contains
	- typically has a Story Focus and a Gameplay Focus, but these are not part of the coding and more part of the design
	- one Act is active at a time, no parallel storylines in this game

Plot Point:
	- main way for things to happen, from a gameplay and/or story standpoint
	- unlike scenes and acts (which are linear and one at a time), Plot Points can happen in different orders depending on player action
	- Plot Points can be dependent on other Plot Points being completed
	

# Story Outline

Details heavily subject to change.

## Scene One

### Act One - An Ordinary Night
- Story Focus: Protagonist is an ordinary man in a suburban house with a wife and child
- Gameplay Focus: Basic navigation, using light switches, opening doors
- Summary
    - It's time for bed. The child is asleep, the wife is staying up late playing video games.
    - You need to turn off the Kitchen and Living Room lights before going to bed
    - Other rooms are blocked off to simplify gameplay

### Act Two - What's that noise?
- Story Focus: The protagonist is woken up by a strange noise, but when he goes to investigate, the noise isn't the only strange thing.
- Gameplay Focus: Full exploration, reading notes
- Summary
    - you are woken up by a noise outside
    - when you look out the window, a huge alien-like creature appears in the sky, bigger than the sun or moon in the sky
    - an instant later, it vanishes and the noise stops
    - the player is now free to explore the house
    - if the player enters the child's bedroom, or the basement office where his wife is, there will be a strange abstract "being" instead
    - the being will first interact as if they are the child/wife, but when the player responds with shock, the abstract being "glitches" and stops
    - if player leaves the house, they will discover a strange "force-field" at the edge of the property
    - after seeing either false-wife or false-child, the player will find a note saying "need dark"
        - if the player turns off a light, another note will appear, "still light. dark please."
        - after turning off all lights, the note will say "meet me in garage"
    - if player does, they are met by the Dark Spirit (aka the Director) who explains (somewhat) the situation and hints about the approaching
      "Light Spirit" who will try to destroy you

### Act Three - Are you Afraid of the Light?
- Story Focus: The "Light Spirit" (aka the Producer) is approaching soon. The player must try to survice.
- Gameplay Focus: The player must explore, prepare, and find ways to defend against the Light Spirit
- Summary: 
    - first main gameplay loop for the Light Spirit - `Instruction\LightSpirit\2026_02_08_LightSpirit.txt`
    - scene ends when player is able to damage it enough during its Full-Presence phase 

## Scene Two

### Act One - The Backstage
- Story Focus: With the Producer temporarily disabled, the Director grants you access to the "BackStage", where the falseness of reality is apparant
- Gameplay Focus: Exploration, meeting characters, collecting items
- Summary:
    - the "Backstage" lets you view your world from the "outside" as if it were a scene in a play or movie
   
### Act Two - Hit the Road
- Story Focus: We learn that there are reusable "sets" for common things such as roads, which can simulate larger areas
- Gameplay Focus: Exploration

### Act Three
