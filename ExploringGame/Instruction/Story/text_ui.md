# GOAL
Show text to the user, for flavor text and dialoge 

# TECHNICAL DETAILS
- use SprintFonts
    - possible fonts to try:
        Bahnschrift
        Book Antiqua
        Bookman Old Style
        Cambria
        Candara
        Carlito
        Centaur
        Century
        Constantia
        DejaVu Serif
        Garamond
        Microsoft New Tai Lue
        Roboto
        Rockwell
- render as a final pass - text should appear over everything else 

# APPEARANCE
- render text in multiple passes
    - render first slightly larger, tinted a certain color, and tranparent, to give a "glowing effect"
    - then render in white 
- text appears at the bottom of the screen, to a maximum of three lines
- text is "typed" one character at a time
- when text is dialogue, the color and alignment of the text depends on who is speaking
    - player dialog is left aligned and uses the same color as flavor text (blue)
    - character dialog is right aligned and uses red tint

# BEHAVIOR
- text will render up to three lines
    - if there is more text, the player can press Space to see more
    - existing text will be cleared and the new text will appear

# CODE
- TextManager class 
