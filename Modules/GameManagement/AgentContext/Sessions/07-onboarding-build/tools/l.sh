#!/bin/bash
# usage: l.sh Entry [args...]  -> runs Lab.Entry in the Lab
S=/c/Users/avig7/AppData/Local/Temp/claude/598d592dadc35d6e/95ba92f3-5e40-42e0-98ca-83606a7598ac/scratchpad
cd /c/Users/avig7/Desktop/AllUnity/ActiveProjects/Abstract_Pixel_Lab
entry=$1
shift
if [ $# -gt 0 ]; then
  json=$(python -c "import sys,json; print(json.dumps(sys.argv[1:]))" "$@")
  timeout 60 unity command script run --file "$(cygpath -w $S/Lab.cs)" --entry "Lab.$entry" --args "$json" 2>&1 | grep -v "^Finding\|^diagnostics\|^compileMs\|^executeMs\|^assemblyName"
else
  timeout 60 unity command script run --file "$(cygpath -w $S/Lab.cs)" --entry "Lab.$entry" 2>&1 | grep -v "^Finding\|^diagnostics\|^compileMs\|^executeMs\|^assemblyName"
fi
