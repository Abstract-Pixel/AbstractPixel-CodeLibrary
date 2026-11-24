# Sample scene groups never reach a build

<!-- layout new -->

<!-- decision -->

The scene system puts every scene of a scene group into Build Settings, which would ship the library's samples inside a game. A scene group can be marked as a sample scene group instead.

## The decision

- **Build Settings:** the scene system never writes a sample scene group's scenes there.
- **In Play:** the Editor opens them with `EditorSceneManager.LoadSceneAsyncInPlayMode`.
- **On the page:** the Scene System page lists them in their own section.
- **Builds:** a build step stops any build that still contains one (Build Settings, a Build Profile or Addressables), naming the scene, with a fix button on the page.
- **Why stop, not remove:** a scripted build cannot change its scene list at that point, and the mistake should be seen.
- **Shared scenes:** a scene used by a sample scene group and a game scene group is an error.
