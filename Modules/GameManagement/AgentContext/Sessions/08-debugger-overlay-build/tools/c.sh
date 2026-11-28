#!/bin/bash
# usage: c.sh Entry [args...] -> runs Chk.Entry in the Lab
S=/c/Users/avig7/AppData/Local/Temp/claude/598d592dadc35d6e/221c4236-b92e-4f8d-881e-a35125d35354/scratchpad
cd /c/Users/avig7/Desktop/AllUnity/ActiveProjects/Abstract_Pixel_Lab
entry=$1
shift
args=()
if [ $# -gt 0 ]; then
  args=(--args "$(python -c "import sys,json; print(json.dumps(sys.argv[1:]))" "$@")")
fi
timeout 60 unity command script run --file "$(cygpath -w $S/Chk.cs)" --entry "Chk.$entry" "${args[@]}" 2>&1 | grep -v "^Finding\|^diagnostics\|^compileMs\|^executeMs\|^assemblyName\|^success\|^command"
