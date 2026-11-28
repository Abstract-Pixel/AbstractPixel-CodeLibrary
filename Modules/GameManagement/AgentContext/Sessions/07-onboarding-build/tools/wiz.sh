#!/bin/bash
# usage: wiz.sh FOLDER "menu ops" "path ops" [noset]
# ops are space-free words: name|index|value (Set) , separated by commas. Runs the first-run window to Ready, then Set up unless "noset".
S=/c/Users/avig7/AppData/Local/Temp/claude/598d592dadc35d6e/95ba92f3-5e40-42e0-98ca-83606a7598ac/scratchpad
d=$S/d.sh
W=OnboardingWindow
FOLDER="$1"
MENUOPS="$2"
PATHOPS="$3"
NOSET="$4"
runops() {
  IFS=',' read -ra OPS <<< "$1"
  for op in "${OPS[@]}"; do
    [ -z "$op" ] && continue
    IFS='|' read -r n i v <<< "$op"
    $d Set $W "$n" "$i" "$v" | sed 's/^/   set /'
  done
}
$d Open $W "Tools/External Project Tools/Game Management/Set Up Game Management" 620 640 >/dev/null; sleep 2
$d Click $W nextButton 0 >/dev/null; sleep 1
$d Set $W folderPath 0 "$FOLDER" >/dev/null
$d Click $W nextButton 0 >/dev/null; sleep 1
runops "$MENUOPS"
$d Click $W nextButton 0 >/dev/null; sleep 1
runops "$PATHOPS"
$d Click $W nextButton 0 >/dev/null; sleep 1
$d Click $W nextButton 0 >/dev/null; sleep 1
echo "--- READY"
$d Dump $W | grep -v "^ *[0-9]\.\?$\|Game Management\|^result\|^size\|button\] Back\|Step"
if [ "$NOSET" == "noset" ]; then exit 0; fi
$d Click $W nextButton 0 >/dev/null; sleep 4
echo "--- ALL SET"
$d Dump $W | grep -v "^ *[0-9]\.\?$\|Game Management\|^result\|^size\|button\] Back"
