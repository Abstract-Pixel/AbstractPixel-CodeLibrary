# A failed scene change goes to the fallback scene group

<!-- layout new -->

<!-- decision -->

When a scene of the target group fails to load, the old level may already be closed and the screen covered, so leaving the player there means a stuck screen.

## The decision

- **Record:** the scene system records what is really open and returns Failed with the scene that failed.
- **Fallback:** it goes to the fallback scene group named in the scene system settings, the start scene group by default.
- **No retry:** a failed load is never tried again. A scene missing from the build or with a broken address fails the same way twice.

## Consequences

- **No loops:** if the fallback itself fails, or the failing change was already going to the fallback, the screen is shown on whatever is open and the error is logged.
- **Preload:** a failed preload returns Failed and goes nowhere, because the player is still in the open level.
- **Start-up:** a failed start-up keeps its own path (`SceneActions.HasStartUpFailed`).
- **Closing:** a scene that fails to close is logged and the change carries on.
