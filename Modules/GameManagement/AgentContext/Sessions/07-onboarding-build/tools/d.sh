#!/bin/bash
# usage: d.sh Entry arg1 arg2 ...   (args become a JSON string array)
S=/c/Users/avig7/AppData/Local/Temp/claude/598d592dadc35d6e/95ba92f3-5e40-42e0-98ca-83606a7598ac/scratchpad
cd /c/Users/avig7/Desktop/AllUnity/ActiveProjects/Abstract_Pixel_Lab
entry=$1; shift
json=$(python -c "
import sys,json
a=sys.argv[1:]
out=[]
for x in a:
    try: out.append(int(x))
    except: out.append(x)
print(json.dumps(out))" "$@")
timeout 60 unity command script run --file "$(cygpath -w $S/Drive.cs)" --entry "Drive.$entry" --args "$json" 2>&1 | grep -v "^Finding\|^diagnostics\|^compileMs\|^executeMs\|^assemblyName"
