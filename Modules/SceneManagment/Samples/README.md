# Scene System Samples

<!-- layout new -->


## Before you start
- These samples are for the Editor only. Their scene groups are sample scene groups: their scenes never go into a build, and a build that would contain one stops and names it.
- Set up the scene system first: Tools > External Project Tools > Scene System > Set Up Scene System.
- Open each sample's scenes with the scene picker in the main toolbar. On 6.3 it is hidden until you right-click an empty part of the toolbar and tick "Abstract Pixel/Scene picker"; on 7.0 it is in the toolbar's overflow list.
- Samples 01 to 04, 06, 07, 08 and 10 use `Shared/Scenes/Samples Managers` as their manager scene: it sets up the basic fade.

## Samples
| Sample | Level | You learn |
|---|---|---|
| [01 Button Changes Level](01%20Button%20Changes%20Level/README.md) | 0 no code | A UI button changes the scene group with a fade |
| [02 Change Level From Code](02%20Change%20Level%20From%20Code/README.md) | 1 daily code | SceneActions.ChangeTo, its result, and the scene messages in order |
| [03 Fast Restart](03%20Fast%20Restart/README.md) | 1 daily code | Retry and Reset shared scenes |
| [04 Play From Any Scene](04%20Play%20From%20Any%20Scene/README.md) | 0 no code | Play from any scene of a group; the scene opening preferences and setup |
| [05 Persistent Player And Spawn Points](05%20Persistent%20Player%20And%20Spawn%20Points/README.md) | 1 daily code | Relocatables, labels and spawn points |
| [06 Loading Scene](06%20Loading%20Scene/README.md) | 2 choices | A loading scene with a progress bar of primitives |
| [07 Preload The Next Level](07%20Preload%20The%20Next%20Level/README.md) | 2 choices | Preload, then an instant change |
| [08 Addressables](08%20Addressables/README.md) | 3 full control | An Addressable main scene and the moves |
| [09 Editor Tools](09%20Editor%20Tools/README.md) | 0 no code | Create Scene Group, the inspector, the Debugger, the scene picker |
| [10 Launch Scene](10%20Launch%20Scene/README.md) | 2 choices | A splash that holds the first group, with a Timeline |

## Coverage
| ID | Feature | Sample | Look only |
|---|---|---|---|
| F01 | Setup window | 04 | |
| F02 | Page: Setup section | 04 | look only |
| F03 | Page: Start-up section | 10 | look only |
| F04 | Page: Scene sets section | 04 | look only |
| F05 | Page: New scenes section | 08 | look only |
| F06 | Page: Loading scenes section | 06 | look only |
| F07 | Page: Relocation section | 05 | look only |
| F08 | Page: Scene Groups Debugger | 09 | |
| F09 | Page: Sample scene groups section | 09 | |
| F10 | Page: Preview migration and Migrate | 09 | look only |
| F11 | Scene group inspector | 09 | |
| F12 | Group check and fix buttons | 09 | |
| F13 | Scene set inspector, overrides, Revert to set | 04 | look only |
| F14 | Scene field | 09 | |
| F15 | Create Scene Group | 09 | |
| F16 | Scene picker | 09 | |
| F17 | Scene opening preferences | 04 | |
| F18 | Sample scene group and the build guard | 09 | |
| F19 | Relocatable, Relocation Label, Stamp relocation label IDs | 05 | |
| F20 | Spawn point inspector and Scene view tools | 05 | |
| F21 | MonoSceneActions on a UnityEvent | 01 | |
| F22 | Basic fade, TransitionProfile, Screen counts as visible at | 01 | |
| F23 | SceneActions.ChangeTo and SceneChangeResult | 02 | |
| F24 | Scene messages and ScreenState | 02 | |
| F25 | Failed change and the fallback scene group | 02 | look only |
| F26 | Retry and Reset shared scenes | 03 | |
| F27 | Start-up and Play from any scene | 04 | |
| F28 | Relocation: Hold, Relocate, Release, missing position | 05 | |
| F29 | Loading scene, LoadingProgress, LoadingProgressView, ILoadingProgressDisplay | 06 | |
| F30 | Preload and the preloaded switch | 07 | |
| F31 | Addressables loader and moves | 08 | |
| F32 | Launch scene hold and its SceneActions calls | 10 | |
| F33 | Launch scene helpers (Marker, Receiver, TimelineClock, RenderSettingsGuard, StartUpLog) | 10 | |
