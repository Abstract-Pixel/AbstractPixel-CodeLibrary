# Addressables and BroAudio are optional

The scene and transitions modules compile and build without the Addressables package and without BroAudio. Code that uses them sits behind defines set by the asmdef's `versionDefines`, so a new project that installs neither still builds. Rally Shift uses both, but a small survivors-like or horror game may use neither.

## Consequences

Without Addressables every scene loads from Build Settings, and a scene that is only Addressable reports a clear error naming the scene and the missing package. The Addressables sample stops with that error instead of failing to compile, and its README says the package is required.
