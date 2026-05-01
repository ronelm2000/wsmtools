# AGENTS.md

## Build & Test

```ps
dotnet build
dotnet test --filter TestCategory!=Manual
```

Note: `IDE0005` (unnecessary using directive) is already configured as an error in `.editorconfig` (line ~222). The `dotnet format --include <files>` pre-commit step will automatically fix these.

### Pre-commit

Run `dotnet format` on affected files before committing:

```ps
dotnet format --include <list_of_affected_files>
```

Example:
```ps
dotnet format --include Montage.Weiss.Tools/CLI/ExportVerb.cs Montage.Card.API/Interfaces/Inputs/IConsole.cs
```

## Projects

| Project | Type | Entry |
|---------|------|-------|
| Montage.Weiss.Tools | CLI | `wstools` (net10.0) |
| Montage.Weiss.Tools.GUI | Avalonia | `wsm-gui.exe` (net10.0-windows) |
| Montage.Card.API | Library | - |
| Montage.Weiss.Tools.Test | Tests | MSTest |
| Montage.Weiss.Tools.GUI.Test | Tests | NUnit |

GUI project depends on Tools + Card.API.

## Publish

```ps
# CLI
dotnet publish ./Montage.Weiss.Tools/Montage.Weiss.Tools.csproj -c Release -r win-x64 -o publish -p:PublishSingleFile=true --self-contained true

# GUI
dotnet publish ./Montage.Weiss.Tools.GUI/Montage.Weiss.Tools.GUI.csproj -c Release -f net10.0-windows -r win-x64 -o publish -p:PublishSingleFile=true --self-contained true
```

## Style

- File-scoped namespaces (`.editorconfig` enforces `csharp_style_namespace_declarations = file_scoped`)
- `var` preferred everywhere
- Allman braces

## Pre-Implementation

Before starting any implementation work:
- Check `.editorconfig` and ensure `dotnet_diagnostic.IDE0005.severity = error` is set.

## File I/O

- Prefer `Fluent.IO.Path` over `System.IO` for file operations
- Use `FluentPathExtensions` (in `Montage.Weiss.Tools/Utilities/`) for missing `System.IO.Path` methods
- Available helpers: `GetTempFilePath(extension)`, `WriteStringAsync()`, `Delete()`, `CreateFileStream()`, etc.

## Commit Messages

Use `Co-Authored-By` trailer for AI attribution. Format:

```
Co-Authored-By: <AI Tool Name> <email>
```

Examples for different AI tools:

**OpenCode:**
```
Refactor deck exporter pipeline

Extracted common export logic into base class.
Co-Authored-By: OpenCode <noreply@opencode.ai>
```

**Claude:**
```
Refactor deck exporter pipeline

Extracted common export logic into base class.
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Copilot:**
```
Refactor deck exporter pipeline

Extracted common export logic into base class.
Co-Authored-By: Copilot <noreply@github.com>
```

Use the appropriate `Co-Authored-By` line based on which AI tool assisted with the changes.

## Package Management

Centralized in `Directory.Packages.props`. Floating versions enabled.

```ps
# Update packages after any project change
dotnet nuget add source https://api.nuget.org/v3/index.json
dotnet restore
```

## Interface Implementations

Implementations of generic interfaces from `Montage.Card.API` like `IDeckExporter<D,C>` go in `Impls/` under project `Montage.Weiss.Tools`. Format:

```
Montage.Weiss.Tools/
└── Impls/
    └── Exporter/
        └── Deck/
            ├── CockatriceDeckExporter.cs   # IDeckExporter<WeissSchwarzDeck, WeissSchwarzCard>
            └── LocalDeckJSONExporter.cs
    └── Parser/
        └── Deck/
            └── CockatriceDeckParser.cs
```

Pattern: `Montage.Weiss.Tools/Impls/<VerbNoun>/<Input>/<OriginPlatform><Input><VerbNoun>.cs`

- VerbNoun = Exporter / Parser / Importer / etc.
- Input = Deck / Card / List / etc.
- OriginPlatform = target platform name in PascalCase

Large exporters (1000+ LoC OR 5+ inner classes): use nested package `<OriginPlatform>/<OriginPlatform><Input><VerbNoun>.cs` instead and expand all inner classes into separate files on the same package.

Example: `Montage.Weiss.Tools/Impls/Exporter/Deck/TTS/TTSDeckExporter.cs`

## HOTC Parsing Corrections

When Heart of the Cards (HOTC) has incorrect data, add manual overrides in `Montage.Weiss.Tools/Impls/Parsers/Cards/HeartOfTheCardsURLParser.cs`.

Available correction methods:
- `HandleColorCorrections()` - Fix incorrect color parsing (line ~337)
- `HandleRarityCorrections()` - Fix incorrect rarity parsing (line ~347)
- `HandleCorrections()` - Fix incorrect side/type parsing (line ~362)

Pattern: Add a new case to the switch expression with the card serial and correct value.

```csharp
private CardColor HandleColorCorrections(string serial, Exception innerException)
{
    return serial switch
    {
        "SG/W70-106" => CardColor.Blue,
        "VA/WE30-55" => CardColor.Red,
        "CC/S48-056" => CardColor.Red,  // Example: HOTC error, manually set to Red
        _ => throw new NotImplementedException($"Unsupported color correction for {serial}.", innerException)
    };
}
```

After adding a correction, verify with `dotnet build` and `dotnet test --filter TestCategory!=Manual`.