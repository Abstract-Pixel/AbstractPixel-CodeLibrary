# Coding Standards

Shared by Abstract Pixel Library and Project Rally Shift (Rally embeds this library as a submodule). Read this at **review time** (`/code-review`, `/simplify`, `rally-review`, `ui-review`) — implementation agents follow `CLAUDE.md`'s navigation and the verification bar below; everything else here is judged against the finished diff, not self-policed mid-task.

## Priority order

When standards conflict, resolve in this order:

1. **Correctness** — no regressions, no bugs introduced.
2. **Matches conventions** — follows the codebase's existing architecture and patterns; don't invent a second way to do something already solved.
3. **Simplicity** — YAGNI, DRY, no speculative generality.
4. **Maintainability** — readable at a glance, consistent style.

## Verification bar (minimum, every task)

Before a task counts as done:

- **Compiles cleanly.** No errors; warnings addressed or explicitly called out.
- **Exercised once as a developer or player would** — through the Editor UI, Play mode, or the real game flow, with real input and settings. Not: calling the method directly, invoking a button only through a test harness, or an agent probe script standing in for real use.
- This is a basic real-use smoke check, **not a hardening pass**. Deliberate edge-case hunting, stress tests and bug sweeps are a separate, later task — don't fold them into this one uninvited.
- **Exception:** write an edge-case test when it is cheap and the case is obviously relevant (off-by-one, null, empty collection). A trivial test that only exercises the happy path, and would pass on broken code, is the exact failure this bar exists to catch.

## Code-writing standards

- **YAGNI.** Build what the task needs, not what it might need. No speculative config, abstraction layers, or hooks for a future nobody asked for.
- **DRY, but don't force it.** Don't duplicate logic that already exists elsewhere — reuse or extend it (see the library's "check it doesn't exist already" rule). Don't invent a shared abstraction for two call sites that merely look alike today but may diverge.
- **Readable at a glance.** A developer who has never seen this system should be able to read one function and understand what it does, without opening five other files first. Favor named intermediate variables and early returns over nested conditionals and dense one-liners.
- **No cramming code onto one line.** This includes short `if` statements: always a braced, multi-line body, never a single-line guard clause.

  ```csharp
  // Before (banned)
  if (player == null) return;

  // After (required)
  if (player == null)
  {
      return;
  }
  ```

- **Consistent architecture.** Follow the design pattern the surrounding code already uses (ServiceLocator, event buses, command-based UI animation, etc.) the same way across sessions. Don't introduce a second pattern for something already solved one way.

## Already covered elsewhere (don't duplicate here)

Enforced by the library's `CLAUDE.md`, stays there:

- One type per file; files kept to about 400 lines.
- Plain English: no `var`, full words from the module's glossary, comments say why not what.
- Version guards (`#if UNITY_7000_0_OR_NEWER`) for Unity-version-specific APIs.
- Domain-reload-off: statics reset via `RuntimeInitializeOnLoadMethod`.
- Optional-package guards (`versionDefines`), one clear error when the package is missing.
- Saved data survives renames (`FormerlySerializedAs`, `MovedFrom`, `git mv` with `.meta`).

## Project-specific additions

See each project's own `CLAUDE.md` for conventions that don't apply universally (naming style, performance targets, etc.).
