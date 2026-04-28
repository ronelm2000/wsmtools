# AGENTS.md

## Build & Test

```ps
dotnet build
dotnet test --filter TestCategory!=Manual
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

## Commit Messages

Use `Co-Authored-By` trailer for AI attribution:

```
Co-Authored-By: OpenCode <noreply@opencode.ai>
Co-Authored-By: Copilot <noreply@github.com>
```

Example:
```
Refactor deck exporter pipeline

Extracted common export logic into base class.
Co-Authored-By: OpenCode <noreply@opencode.ai>
```

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