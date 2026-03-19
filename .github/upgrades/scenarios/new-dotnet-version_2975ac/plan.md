# .NET 10 Upgrade Plan for wsmtools

## Table of Contents

- [1 Executive Summary](#executive-summary)
- [2 Migration Strategy](#migration-strategy)
- [3 Detailed Dependency Analysis](#detailed-dependency-analysis)
- [4 Project-by-Project Plans](#project-by-project-plans)
- [5 Package Update Reference](#package-update-reference)
- [6 Breaking Changes Catalog](#breaking-changes-catalog)
- [7 Testing & Validation Strategy](#testing--validation-strategy)
- [8 Risk Management & Mitigation](#risk-management--mitigation)
- [9 Source Control Strategy](#source-control-strategy)
- [10 Success Criteria](#success-criteria)
- [11 Appendix: Assessment Summary](#appendix-assessment-summary)

---

## 1 Executive Summary

Selected Strategy
- **All-At-Once Strategy** — All projects in the solution will be upgraded simultaneously in a single atomic operation.

Rationale
- Solution size: 4 projects (small)
- Current state: 3 projects target `net9.0`, 1 project already targets `net10.0`
- Assessment shows most packages are compatible (37/43) and only a small subset require updates
- Test project exists (`Montage.Weiss.Tools.Test`) enabling post-upgrade validation

Scope
- Projects: `Montage.Card.API` (class library), `Montage.Weiss.Tools.GUI` (WinForms/Avalonia), `Montage.Weiss.Tools.Test` (test runner), `Montage.Weiss.Tools` (CLI app)
- Target framework: `.NET 10.0 (net10.0)`; GUI project proposed `net10.0-windows` per assessment
- Package updates: include EF Core → `10.0.5`, System.Text.Json → `10.0.5`, Microsoft.Extensions.* → `10.0.5`, Microsoft.Extensions.DependencyInjection.Abstractions → `10.0.5` and others listed in §5

Key risks
- Behavioral changes around `System.Uri` usage (multiple occurrences) — requires runtime testing
- One binary-incompatible API reported (affects `Montage.Card.API`) — requires code adjustment
- Deprecated NuGet (`Avalonia.ReactiveUI`) used by GUI project — requires replacement or follow-up

Deliverable
- Atomic plan that, when executed, results in all projects targeting the proposed frameworks with all recommended package updates applied and the solution building and tests passing.


## 2 Migration Strategy

Approach
- All projects will be updated simultaneously (atomic operation). This includes TargetFramework updates, all NuGet package version updates from the assessment, dependency restores, build and fix pass, and running tests.

Key principles (applied)
- Update all project files and central MSBuild imports (e.g., Directory.Build.props) in one coordinated change set.
- Apply all package updates recommended by assessment — do not skip package updates flagged in assessment.
- Build the full solution and address compilation errors in the same atomic change set.
- Run tests after the atomic upgrade completes and address test failures.

Prerequisites (must be completed before executing the atomic upgrade)
- Ensure .NET 10 SDK is installed on the machine that will perform the upgrade. If `global.json` exists, update or validate it to reference a compatible SDK for `net10.0`.
- Ensure working branch is prepared: pending changes action = commit (per assessment defaults).
- Create and switch to feature branch: `upgrade-to-NET10` (single branch for the atomic change).


## 3 Detailed Dependency Analysis

Summary
- Total projects: 4
- Dependency relationships (from assessment):
  - `Montage.Weiss.Tools` (net10.0) → depends on `Montage.Card.API` (net9.0)
  - `Montage.Weiss.Tools.GUI` (net9.0) → depends on `Montage.Card.API` (net9.0) and `Montage.Weiss.Tools` (net10.0)
  - `Montage.Weiss.Tools.Test` (net9.0) → depends on `Montage.Weiss.Tools` (net10.0)

Implication for All-At-Once
- Because the upgrade is atomic, we do not perform phased per-project upgrades. However, understanding the dependency graph is important for diagnosing build/test failures after the atomic change.

Topological considerations
- `Montage.Card.API` is a dependency for other projects — expect any binary-incompatible change here to affect dependants; include it in the breaking-changes catalog for focused fixes during the atomic build pass.


## 4 Project-by-Project Plans

Guidance: Each project's plan below states the current state, target state, and the changes to be made during the atomic upgrade. The executor will perform these changes for all projects in one operation.

### Project: `Montage.Card.API` (Montage.Card.API\Montage.Card.API.csproj)
- Current Target Framework: `net9.0`
- Target Framework: `net10.0`
- Project type: ClassLibrary (SDK-style)
- Key package updates (from assessment):
  - `Microsoft.EntityFrameworkCore` 9.0.9 → 10.0.5
  - `Microsoft.EntityFrameworkCore.Relational` 9.0.9 → 10.0.5
  - `Microsoft.Extensions.Caching.Memory` 9.0.9 → 10.0.5
  - `System.Text.Json` 9.0.9 → 10.0.5
  - Note: `Microsoft.Win32.Registry` currently referenced (5.0.0); assessment notes framework now includes functionality — evaluate removing explicit package and use built-in reference if applicable.
- Expected breaking issues: 1 binary-incompatible API reported (Linq.AsyncEnumerable related) — requires code change at compile time if surfaced.
- Migration steps (to be executed as part of atomic upgrade):
  1. Update `<TargetFramework>` to `net10.0`.
  2. Update PackageReference versions to the exact target versions listed above.
  3. Restore packages and build solution; address compile-time errors caused by framework or package changes.
  4. Run unit tests (post-atomic upgrade) that exercise this library via dependent projects.
- Validation: project compiles; no warnings treated as errors only if configured; unit tests passing where applicable.

---

### Project: `Montage.Weiss.Tools.GUI` (Montage.Weiss.Tools.GUI\Montage.Weiss.Tools.GUI.csproj)
- Current Target Framework: `net9.0`
- Proposed Target Framework: `net10.0-windows` (GUI/WinForms/Avalonia scenario)
- Project type: WinForms / Avalonia (SDK-style)
- Key package notes:
  - `Avalonia.ReactiveUI` is marked deprecated in the assessment — identify replacement or adjust code to use supported ReactiveUI package if needed.
  - Most other UI packages are compatible.
- Expected breaking issues: multiple behavioral changes (~42) and 2 source-incompatible items (requires code recompilation and fixes).
- Migration steps:
  1. Update `<TargetFramework>` to `net10.0-windows`.
  2. Update any package references if a suggested version is listed (follow §5).
  3. Replace or remove deprecated `Avalonia.ReactiveUI` dependency (investigate impact before replacement) — flag as manual review if replacement not straightforward.
  4. Restore and build in the atomic pass and fix compilation issues.
  5. Run UI-related automated tests (if available) and perform focused review for `System.Uri` behavioural areas called out by assessment.
- Validation: GUI project builds; unit/automated UI tests (if any) pass; manual verification may be required outside this plan (not included as an automated task).

---

### Project: `Montage.Weiss.Tools.Test` (Montage.Weiss.Tools.Test\Montage.Weiss.Tools.Test.csproj)
- Current Target Framework: `net9.0`
- Target Framework: `net10.0`
- Project type: Test project (SDK-style)
- Key package updates:
  - `Microsoft.Extensions.DependencyInjection.Abstractions` 9.0.9 → 10.0.5
  - Test SDKs remain compatible (`Microsoft.NET.Test.Sdk`, `MSTest.*`)
- Migration steps:
  1. Update `<TargetFramework>` to `net10.0`.
  2. Update package references to suggested versions.
  3. Restore and run test suite after the atomic upgrade.
- Validation: All unit tests pass in `Montage.Weiss.Tools.Test`.

---

### Project: `Montage.Weiss.Tools` (MontageWeissTools\Montage.Weiss.Tools.csproj)
- Current Target Framework: `net10.0` (already on target)
- Project type: DotNetCoreApp (CLI)
- Key package updates recommended in assessment:
  - `Microsoft.Extensions.Caching.Memory` 9.0.9 → 10.0.5
  - `System.Text.Json` 9.0.9 → 10.0.5
- Migration steps (included in atomic pass):
  1. Confirm and update PackageReference entries to target versions where recommended.
  2. Restore and build together with other projects.
- Validation: CLI project builds; integration tests (if any) pass.


## 5 Package Update Reference

Group updates (apply to all projects simultaneously):

### Core framework-aligned updates (affecting multiple projects)
- `Microsoft.EntityFrameworkCore` 9.0.9 → 10.0.5 (affects: `Montage.Card.API`)
- `Microsoft.EntityFrameworkCore.Relational` 9.0.9 → 10.0.5 (affects: `Montage.Card.API`)
- `Microsoft.Extensions.Caching.Memory` 9.0.9 → 10.0.5 (affects: `Montage.Card.API`, `Montage.Weiss.Tools`)
- `Microsoft.Extensions.DependencyInjection.Abstractions` 9.0.9 → 10.0.5 (affects: `Montage.Weiss.Tools.Test`)
- `System.Text.Json` 9.0.9 → 10.0.5 (affects: `Montage.Card.API`, `Montage.Weiss.Tools`)

### Deprecated/Framework-included notes
- `Microsoft.Win32.Registry` referenced as `5.0.0` — assessment notes functionality included with framework; consider removing explicit NuGet and using framework reference where available (validate at compile time).
- `Avalonia.ReactiveUI` marked deprecated — review replacement (ReactiveUI package or updated Avalonia integration) before executing automated replacement.

### All other packages
- 37 packages were reported compatible in the assessment. Keep existing versions unless the assessment lists a suggested upgrade.


## 6 Breaking Changes Catalog

Top items to inspect and likely fixes during the atomic build pass:

- `System.Uri` behavioral changes (multiple occurrences across projects):
  - Review code that constructs `Uri` from strings; validate `AbsolutePath`, encoding, and `ToString()` usage. Add explicit `UriKind` or normalize URIs if tests show changes.

- Binary-incompatible API reported for `System.Linq.AsyncEnumerable` (affects `Montage.Card.API`):
  - Identify usages of types from `System.Linq.Async`/`System.Interactive.Async` and adjust to API surface in the upgraded package or use compatibility shims.

- `string.Join(string, ReadOnlySpan<string>)` source-incompatible occurrences:
  - Replace with array-based overloads or adjust call sites where the ReadOnlySpan overload is not available under the new compilation model.

- Avalonia/ReactiveUI deprecation:
  - If `Avalonia.ReactiveUI` removed or replaced, update view-model wiring and reactive bindings as required by replacement package.

- EF Core 9→10 changes:
  - Review EF Core 10 migration notes for any change in configuration, logging, or provider behavior (SQLite provider compatibility). Regenerate any EF migrations if necessary and validate DB access.

Note: The full set of expected breaking changes is discovery-driven — additional compile-time errors may appear during the build step and must be resolved in the single atomic upgrade pass.


## 7 Testing & Validation Strategy

All-at-once testing flow
1. After performing the atomic upgrade (framework + packages), run a full `dotnet restore` and a solution build to identify compile-time errors.
2. Run unit tests in `Montage.Weiss.Tools.Test` (test runner) and any other test projects discovered by the test discovery step.
3. Validate integration points:
   - Library consumers (ensure `Montage.Card.API` APIs remain compatible with dependants)
   - CLI scenarios exercised by integration/unit tests
4. Focused runtime checks:
   - Tests or checks covering `System.Uri` usage paths
   - EF Core query behavior and database access

Validation checklist (per project)
- [ ] Project TargetFramework updated to proposed value
- [ ] Packages updated according to §5
- [ ] Project builds without errors
- [ ] Unit tests pass (where present)
- [ ] No outstanding security-vulnerability package flags remaining from assessment

Note: Manual UI validation is outside the scope of this automated plan. The plan flags GUI issues for manual review.


## 8 Risk Management & Mitigation

Project risk summary
- `Montage.Weiss.Tools.GUI` — Medium risk: large number of behavioral/source changes (44+ LOC impact). Mitigation: allocate manual review for deprecated package and run targeted UI tests.
- `Montage.Card.API` — Medium risk: binary-incompatible API reported. Mitigation: prioritize fixing compile-time errors in the atomic build pass; add compatibility shims where appropriate.
- `Montage.Weiss.Tools.Test` — Low/Medium risk: test framework references updated; run tests early in validation.
- `Montage.Weiss.Tools` — Low risk: already targeting `net10.0`; mainly package alignment.

Roll-back plan (if atomic upgrade fails)
- Because this is an atomic upgrade performed on a feature branch (`upgrade-to-NET10`), revert is a simple branch drop or reset to pre-upgrade commit. Keep a backup commit of original state before applying the atomic change.

Mitigation tactics
- Run `dotnet restore` and `dotnet build` immediately after applying the atomic changes to capture all compile issues.
- Address binary-incompatible issues first, then source incompatibilities, then behavioral issues discovered by tests.
- Flag any manual intervention points (e.g., GUI deprecated package replacement) as items requiring human review rather than automated change.


## 9 Source Control Strategy

Branching
- Start from source branch: `master` (per assessment defaults).
- Create and switch to upgrade branch: `upgrade-to-NET10`.
- Commit pending local changes before creating the upgrade branch (assessment default: pending changes = commit).

Commit strategy
- The atomic upgrade should be submitted as a single focused change set representing all TargetFramework and package updates plus code fixes required to compile.
- Include a single cohesive commit (or minimal commits grouped by purpose) describing: "Atomic upgrade to .NET 10 (TargetFramework + packages + compile fixes)".

Pull request review
- Create a PR from `upgrade-to-NET10` → `master` and include a checklist referencing this `plan.md` and assessment summary.
- Require at least one code review and CI pipeline to run build and tests before merge.


## 10 Success Criteria

The migration is complete when all of the following are satisfied:
- All projects target their proposed framework (`net10.0` or `net10.0-windows` for GUI as noted).
- All package updates from §5 have been applied.
- Solution builds with 0 compilation errors.
- All automated tests run and pass (unit tests in `Montage.Weiss.Tools.Test` and any other discovered test projects).
- No remaining critical security vulnerabilities in NuGet packages flagged by the assessment.
- Deprecated package issues are either replaced or documented with remaining manual action items.


## 11 Appendix: Assessment Summary

Refer to `.github/upgrades/scenarios/new-dotnet-version_2975ac/assessment.md` for the full automated assessment data used to create this plan.




