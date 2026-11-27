# 07 Onboarding window build (7A-build)

Type: implement. Blocked by: 06. Budget: `plan: gm-finish, step 7`.

## Goal

A new game can be set up from the onboarding window: both paths ("Copy the library's menu", "Use my own") and
running it again to add a skipped menu.

## Read first

- Ticket 01's handoff and stills, `../../Sessions/Session5/GRILL-RESULTS.md` §4, `../../UI-STYLE.md`.

## Done and verified

- In a project with no settings asset: run the window, choose each menu with both paths, enter Play and use every
  menu it made with keys, gamepad and mouse. Run it again and add a skipped menu.
- It opens once per Editor session while no settings asset exists; never in batch mode, Play mode or tests.
- Stills at three widths, each read; module tests pass.
