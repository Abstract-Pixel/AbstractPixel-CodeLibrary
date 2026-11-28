#!/bin/bash
# Blocks until an assembly's DLL is newer than its newest source file. One call instead of a polling loop.
# It does not read the console. On success, still check the console for errors and new warnings.
# A checkout or merge changes file times without changing code, and Unity then keeps the old DLL: this can say
# STALE when nothing is wrong. It never says FRESH for a source file newer than the DLL.
# usage: wait_compile.sh <lab|rally> <assembly name> [timeout seconds, default 300]
LIB="$(cd "$(dirname "$0")/../../../.." && pwd)"
LAB="$(cd "$LIB/../.." && pwd)"
case "$1" in
  lab) PROJECT="$LAB" ;;
  rally) PROJECT="$LAB/../ProjectRallyShift" ;;
  *) echo "usage: wait_compile.sh <lab|rally> <assembly name> [timeout seconds]"; exit 2 ;;
esac

python - "$(cygpath -m "$PROJECT")" "$2" "${3:-300}" <<'PY'
import json, os, sys, time

project, assembly, limit = sys.argv[1], sys.argv[2], int(sys.argv[3])
modules = os.path.join(project, 'LocalPackages', 'AbstractPixelLibrary')
dll = os.path.join(project, 'Library', 'ScriptAssemblies', assembly + '.dll')

asmdefs = {}
for folder, _, files in os.walk(modules):
    for name in files:
        if name.endswith('.asmdef'):
            with open(os.path.join(folder, name), encoding='utf-8-sig') as handle:
                asmdefs[folder] = json.load(handle).get('name')
roots = [folder for folder, name in asmdefs.items() if name == assembly]
if not roots:
    sys.exit('FAIL: no asmdef named %s under %s' % (assembly, modules))

sources = []
for folder, subfolders, files in os.walk(roots[0]):
    # A subfolder with its own asmdef belongs to another assembly.
    subfolders[:] = [s for s in subfolders if os.path.join(folder, s) not in asmdefs]
    sources += [os.path.join(folder, f) for f in files if f.endswith('.cs')]
if not sources:
    sys.exit('FAIL: no .cs files for %s' % assembly)
newest = max(sources, key=os.path.getmtime)

deadline = time.time() + limit
while True:
    if os.path.exists(dll) and os.path.getmtime(dll) >= os.path.getmtime(newest):
        print('FRESH: %s.dll is newer than %s' % (assembly, os.path.relpath(newest, modules)))
        sys.exit(0)
    if time.time() >= deadline:
        sys.exit('STALE after %ds: %s changed after the DLL. Check the console, focus the Editor, look for a pop-up. '
                 'After a checkout or merge with no real code change, Unity keeps the old DLL.'
                 % (limit, os.path.relpath(newest, modules)))
    time.sleep(3)
PY
