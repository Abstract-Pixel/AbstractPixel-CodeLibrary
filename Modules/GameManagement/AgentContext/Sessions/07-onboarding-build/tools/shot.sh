#!/bin/bash
# usage: shot.sh name  -> shots/name.png (Lab only), waits 5 s after foregrounding
S=/c/Users/avig7/AppData/Local/Temp/claude/598d592dadc35d6e/95ba92f3-5e40-42e0-98ca-83606a7598ac/scratchpad
powershell -NoProfile -ExecutionPolicy Bypass -File "$(cygpath -w $S/fg.ps1)" >/dev/null 2>&1
sleep 5
$S/d.sh Grab ${2:-OnboardingWindow} "$(cygpath -m $S/shots/$1.png)"
