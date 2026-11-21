# A failed scene change goes to the fallback scene group

When a scene of the target group fails to load, the old level may already be closed and the screen covered, or the loading scene showing, so leaving the player there means a stuck screen. The scene system records what is really open, returns Failed with the scene that failed, and goes to the fallback scene group named in the scene system settings (the start scene group by default). A failed load is never tried again: a scene missing from the build or with a broken address fails the same way twice.

## Consequences

If the fallback itself fails, or the failing change was already going to the fallback, the screen is shown on whatever is open and the error is logged, so the scene system never loops. A failed preload returns Failed and does not go anywhere, because the player is still in the open level. A failed start-up keeps its own path (`SceneActions.HasStartUpFailed`). A scene that fails to close is logged and the change carries on.
