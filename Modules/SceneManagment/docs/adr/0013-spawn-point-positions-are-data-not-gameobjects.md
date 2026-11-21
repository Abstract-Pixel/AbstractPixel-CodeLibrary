# Spawn point positions are data, not GameObjects

A label can need 100 positions. One GameObject per position makes 100 objects to manage, and child transforms of one spawn point cannot be clicked in the Scene view without expanding the Hierarchy. So a spawn point stores its positions (position, rotation, instance) as data, its own transform being the first, and the Editor draws every position's preview at all times and makes it clickable: one click puts Unity's move or rotate handle on that position, Shift+click on a surface adds one. A designer uses as many spawn points per label as there are areas, not one per object.

## Considered options

- One spawn point GameObject per position. Rejected: not workable at 100 objects.
- Child transforms as positions. Rejected: a bare transform has nothing to click in the Scene view, so editing needs the Hierarchy.
