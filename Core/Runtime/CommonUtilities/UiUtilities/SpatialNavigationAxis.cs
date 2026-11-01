namespace AbstractPixel.Core.UI
{
    // Which directions AutoSpatialNavigation links buttons in.
    public enum SpatialNavigationAxis
    {
        // Up/Down/Left/Right all search the screen for the nearest button in that direction (grids, free layouts).
        Both = 0,

        // One column read top to bottom: Up/Down step through it, Left/Right do nothing.
        // Works for staggered or diagonal stacks where a cone search would also link Left/Right.
        VerticalList = 1,

        // One row read left to right: Left/Right step through it, Up/Down do nothing.
        HorizontalList = 2,
    }
}
