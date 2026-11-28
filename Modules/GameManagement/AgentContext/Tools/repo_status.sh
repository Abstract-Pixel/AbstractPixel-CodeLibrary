#!/bin/bash
# One call for the state of all four repos and both Editors.
# usage: repo_status.sh [--no-editors]
LIB="$(cd "$(dirname "$0")/../../../.." && pwd)"
LAB="$(cd "$LIB/../.." && pwd)"
RALLY="$LAB/../ProjectRallyShift"

for entry in "LAB|$LAB" "LIB|$LIB" "RALLY|$RALLY" "RALLY LIB|$RALLY/LocalPackages/AbstractPixelLibrary"; do
  name="${entry%%|*}"
  path="${entry#*|}"
  if ! git -C "$path" rev-parse HEAD >/dev/null 2>&1; then
    echo "== $name: NOT A GIT REPO at $path"
    continue
  fi
  branch="$(git -C "$path" branch --show-current)"
  echo "== $name  ${branch:-(detached)}  $(git -C "$path" rev-parse --short HEAD)"
  git -C "$path" status --porcelain
done

if [ "$1" != "--no-editors" ]; then
  echo "== Lab Editor"
  (cd "$LAB" && timeout 30 unity command editor status) || echo "no answer: check for a pop-up"
  echo "== Rally Editor"
  (cd "$RALLY" && timeout 30 unity command editor_status) || echo "no answer: check for a pop-up"
fi
