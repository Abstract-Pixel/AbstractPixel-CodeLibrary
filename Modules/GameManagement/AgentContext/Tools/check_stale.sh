#!/bin/bash
# Says which PASS rows in a CHECKS.md are still valid. A row is stale when a file under its "Stale if" paths
# changed since its commit (committed, uncommitted or new). Anything it cannot read counts as stale.
# usage: check_stale.sh <path to CHECKS.md>
LIB="$(cd "$(dirname "$0")/../../../.." && pwd)"
[ -f "$1" ] || { echo "no file: $1"; exit 2; }

# Columns: | Id | Check | Result | Commit | Unity | Input | Stale if | Evidence |
awk -F'|' 'NF >= 10 { for (i = 2; i <= 9; i++) gsub(/^[ \t]+|[ \t\r]+$/, "", $i)
  if ($4 == "PASS") print $2 "\t" $5 "\t" $8; else if ($2 != "Id" && $2 !~ /^-+$/) print $2 "\t=\t" $4 }' "$1" |
while IFS=$'\t' read -r id commit paths; do
  if [ "$commit" = "=" ]; then
    echo "OPEN   $id ($paths)"
    continue
  fi
  if [ -z "$paths" ] || ! git -C "$LIB" rev-parse --verify -q "$commit^{commit}" >/dev/null; then
    echo "STALE  $id (missing paths or unknown commit '$commit')"
    continue
  fi
  read -r -a list <<< "$paths"
  missing=""
  for path in "${list[@]}"; do
    [ -e "$LIB/$path" ] || git -C "$LIB" cat-file -e "$commit:$path" 2>/dev/null || missing="$path"
  done
  if [ -n "$missing" ]; then
    echo "STALE  $id (path not found: $missing)"
    continue
  fi
  changed="$( { git -C "$LIB" diff --name-only "$commit" -- "${list[@]}"
                git -C "$LIB" ls-files --others --exclude-standard -- "${list[@]}"; } | grep -v '\.meta$' | head -3 | tr '\n' ' ')"
  if [ -n "$changed" ]; then
    echo "STALE  $id (changed: $changed)"
  else
    echo "VALID  $id"
  fi
done
