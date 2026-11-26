# Spawn point positions are data, not GameObjects

<!-- layout new -->

<!-- decision -->

A spawn point stores its positions (position, rotation, instance) as data, its own transform being the first.

## The decision

- **Why:** a label can need 100 positions. One GameObject per position makes 100 objects to manage, and child transforms of one spawn point cannot be clicked in the Scene view without expanding the Hierarchy.
- **In the Editor:** every position's preview is drawn at all times and is clickable. One click puts Unity's move or rotate handle on that position; Shift+click on a surface adds one.
- **How many:** a designer uses as many spawn points per label as there are areas, not one per object.

## Considered options

| Option | Why not |
|---|---|
| One spawn point GameObject per position | Not workable at 100 objects |
| Child transforms as positions | A bare transform has nothing to click in the Scene view, so editing needs the Hierarchy |
