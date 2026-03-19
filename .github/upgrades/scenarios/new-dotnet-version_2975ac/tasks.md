# wsmtools .NET 10 Upgrade Tasks

## Overview

This document tracks the execution of an atomic upgrade of all projects in the `wsmtools` solution to .NET 10. The All-At-Once approach updates TargetFramework and package references across all projects in a single coordinated change, followed by build fixes and automated testing.

**Progress**: 0/3 tasks complete (0%) ![0%](https://progress-bar.xyz/0)

---

## Tasks

### [▶] TASK-001: Verify prerequisites
**References**: Plan §2 (Prerequisites)

- [▶] (1) Verify .NET 10 SDK is installed on the build machine per Plan §2
- [ ] (2) Runtime/SDK version meets minimum requirements for `net10.0` (**Verify**)
- [ ] (3) If `global.json` exists, validate or update it to reference a compatible .NET 10 SDK per Plan §2
- [ ] (4) Verify required tooling (dotnet CLI version, NuGet client) is available and compatible (**Verify**)

### [ ] TASK-002: Atomic framework and package upgrade with compilation fixes
**References**: Plan §2 (Approach), Plan §4 (Project-by-Project Plans), Plan §5 (Package Update Reference), Plan §6 (Breaking Changes Catalog), Plan §9 (Source Control Strategy)

- [ ] (1) Update `<TargetFramework>` for all projects listed in Plan §4 to the proposed targets (`net10.0` or `net10.0-windows` for GUI) per Plan §4
- [ ] (2) Update PackageReference versions across all projects per Plan §5 (core updates: EF Core → 10.0.5, System.Text.Json → 10.0.5, Microsoft.Extensions.* → 10.0.5, etc.)
- [ ] (3) Restore dependencies for the solution (`dotnet restore`) and verify restore success (**Verify**)
- [ ] (4) Build the solution to identify compile-time errors (`dotnet build`) per Plan §6
- [ ] (5) Fix all compilation errors caused by framework/package updates (reference Plan §6 Breaking Changes Catalog) and re-run build to verify fixes
- [ ] (6) Solution builds with 0 errors (**Verify**)
- [ ] (7) Commit changes with message: "TASK-002: Atomic upgrade to .NET 10 (TargetFramework + packages + compile fixes)"

### [ ] TASK-003: Run test suite and validate upgrade
**References**: Plan §7 (Testing & Validation Strategy), Plan §10 (Success Criteria)

- [ ] (1) Run tests in `Montage.Weiss.Tools.Test` and any other test projects identified in Plan §7 (`dotnet test`) per Plan §7
- [ ] (2) Fix any test failures (reference Plan §6 Breaking Changes Catalog for common issues)
- [ ] (3) Re-run tests after fixes
- [ ] (4) All tests pass with 0 failures (**Verify**)
- [ ] (5) Commit test fixes with message: "TASK-003: Complete testing and validation"

---
