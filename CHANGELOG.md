# Changelog

### [1.0.2] - 2026-03-04
**Stability, Dependency Management, and Documentation**
- **SaveSystem:** Added `com.unity.nuget.newtonsoft-json` (v3.2.2) as an explicit dependency in `package.json` to support JSON serialization in the SaveSystem module.
- **Architecture:** Disabled "Use GUIDs" in specific Assembly Definitions to prevent potential reference breakage during compilation in restricted environments.
- **Maintenance:** Shifted all historical changelog data from `README.md` to this dedicated `CHANGELOG.md` file.
- **Automation:** Added additional validation checks to `SavableBridgeAutomation.cs` to prevent unnecessary errors.
- **Error Handling:** Implemented explicit, non-crashing error messages in `SavableBridgeAutomation.cs` to help identify configuration issues quickly.
- **Fix:** Created a new default `Debug Data Base` ScriptableObject asset to prevent `DebugManagerRefresher` initialization errors.

### [1.0.1] - 2026-03-03
**Cleanup and Standardization**
- **Refactor:** Added Namespaces (`AbstractPixel.Core`) to all legacy utility scripts to prevent naming collisions.
- **Maintenance:** Deleted `LazySingleton`. It was redundant as `PersistentSingleton` provides a more robust solution.
- **Architecture:** Regenerated all `.meta` files (GUIDs) for transferred scripts to ensure unique identity.
- **Fix:** Corrected naming conventions and fixed typos within `SaveSystem` package folders (e.g., `DataManagement`).

### [1.0.0] - 2026-03-02
**Initial Release**
- **Core:** Initial transfer of base utilities: Object Pooling, Timer Systems, and Custom Attributes.
- **Feature:** Implemented `SaveSystem` module: Serialization wrappers, File System management, and Public API.
- **Structure:** Established Assembly Definition boundaries for Runtime/Editor code isolation.