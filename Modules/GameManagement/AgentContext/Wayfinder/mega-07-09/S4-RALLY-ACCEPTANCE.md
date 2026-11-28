# S4: Rally acceptance of everything built

Runs after ticket 09. Session folder: `../../Sessions/mega-07-09-acceptance/`. Not a map ticket.

## Goal

On the final library commit, every editor UI change works in Project Rally Shift, and Rally is left on that commit.
Earlier steps verified their own parts; this step catches what later work broke or never reached Rally.

## Must-have

1. Rally's library is on the final `game-management-rework` commit, which holds all code from S0-S3
   (`git log` and a diff of `Modules/GameManagement` code against the Lab's `main` prove it).
2. Clean compile and module tests in Rally.
3. [RALLY-DOGFOOD.md](RALLY-DOGFOOD.md) R1-R8 on that commit, one pass each: open, look, use the main path, Undo.
   Do not repeat each step's edge cases.
4. Parity scenarios, once, if any runtime file changed since the last parity run.
5. If a runtime file changed after ticket 08's builds: rebuild the dev player and rerun P2, P4, P6, P7. Otherwise do
   not rebuild.
6. Lab: whole module tests and Sample 01's PlayMode flow test on the final `main`.
7. Fix what fails (tests first), then verify. The verifier always runs, even when nothing needed fixing: its packet
   asks for R1-R8 itself.
8. Handoff, with a table: R1-R8, result, still path. **Next session** says: the mega run is complete; the map's next
   open ticket is 10, which this run does not start.
