# Abstract Pixel Code Library

 framework for Unity projects, focusing on core utilities, architectural foundations, and robust feature modules.

## Installation

### Via Local Disk
1. Open the **Package Manager** in Unity.
2. Click the `+` icon and select **"Add package from disk..."**.
3. Select the `package.json` file in your local repository.

### Via Git URL
1. Open the **Package Manager**.
2. Click the `+` icon and select **"Add package from git URL..."**.
3. Paste the repository URL: `https://github.com/Abstract-Pixel/AbstractPixel-CodeLibrary.git`

---

## Changelog

### [1.0.2]    
** Better Stability For Savable Bridge Automation **
- **Less Error Prone:** Added addtional checks before doing automation logic in (`SavableBridgeAutomation.cs`) to  prevent unessery errors that do not matter.
- **Explicit Error Thrown** if for some reason automation process still fails in  `SavableBridgeAutomation.cs`. it shows a non crashing error, of what is the most likely issue.
- **Small Fix** Made a brand new scriptable opject asset of `Debug Data Base` so it does not cause  `Debug Manager Refresher` to throw and error.

### [1.0.1]
**Cleanup and Standardization**
- **Refactor:** Added Namespaces (`AbstractPixel.Core`) to all legacy utility scripts to prevent naming collisions in production projects.
- **Maintenance:** Deleted `LazySingleton`. It was redundant as `PersistentSingleton` provides a more robust solution for the current architecture.
- **Architecture:** Regenerated all `.meta` files (GUIDs) for scripts transferred from legacy projects. This ensures the Library possesses a unique identity and prevents GUID conflicts when imported into projects containing old script versions.
- **Fix:** Corrected naming conventions and fixed typos within the `SaveSystem` package folders and namespaces (e.g., `DataManagement`).

### [1.0.0]
**Initial Release**
- **Core:** Initial transfer of base Abstract Pixel utility code, including Object Pooling, Timer Systems, and Custom Attributes.
- **Feature:** Implemented and tested the `SaveSystem` module, including Serialization wrappers, File System management, and Public API Facade.
- **Structure:** Established Assembly Definition (`.asmdef`) boundaries for Runtime and Editor code isolation.