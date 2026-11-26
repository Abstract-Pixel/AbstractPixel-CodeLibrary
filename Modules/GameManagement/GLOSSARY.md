# Game management

The words of the game state system. They describe the **target design** ([AgentContext/DESIGN.md](AgentContext/DESIGN.md)).
Today's code still says "register" for entering and uses a number for priority. New names come from this list.

## Language

**State**:
One named situation of the game, such as Main Menu, Play, Pause or Game Over. It is an asset.

**State catalog**:
The ordered list of every state. Higher in the list wins.
_Avoid_: priority number

**Top state**:
The state currently in control. Its effects apply.
_Avoid_: highest state, current state

**Sub-state**:
A state that opens over one of its parents and returns to it on close.

**Parent**:
A state that a sub-state may open over. A sub-state may have several.

**Suspended**:
A parent under an open sub-state: still active, not in control.
_Avoid_: evicted

**Effect**:
Something a state controls: time scale, cursor, HUD or input maps.
_Avoid_: configuration

**Request / Leave**:
Asking the state machine to enter or to exit a state.
_Avoid_: register, activate, unregister, deactivate

**Refusal**:
A request the state machine turned down, with a reason.

**Condition**:
A scene script that makes requests when something happens.

**Reactor**:
A scene script that responds to state changes, such as a menu, audio or HUD.

**Transition journal**:
The recorded list of transitions that the debugger reads.

**Problem**:
A setup mistake the validator found, together with its fixes.
