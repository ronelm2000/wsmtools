# AGENTS.md

## Build & Test

```ps
dotnet build
dotnet test --filter <TestClassFilter>
```

### Testing a specific card

To test for a specific card serial (e.g., `NIK/S117-008`), use substring matching with the `~` operator:

```ps
dotnet test --filter "TestCategory~NIK/S117-008"
```

This runs all tests whose `TestCategory` tag contains `NIK/S117-008`.

Limit coverage to the test classes of added/modified unit tests using `FullyQualifiedName~<ClassName>`:

```ps
dotnet test --filter FullyQualifiedName~NewExporterTest
```

Note: `IDE0005` (unnecessary using directive) is already configured as an error in `.editorconfig` (line ~222). The `dotnet format --include <files>` pre-commit step will automatically fix these.

### Pre-push

Build and full non-manual test suite run automatically via `pre-push` hook. To run manually:

```ps
dotnet build
dotnet test --filter TestCategory!=Manual
```

If hooks fail unexpectedly (e.g. running outside a repo), skip them with `--no-verify`:

```ps
git commit --no-verify
git push --no-verify
```

### Git Hooks Setup

Sample hook scripts are in `agent-docs/samples/`:
- `agent-docs/samples/pre-commit` — runs `dotnet format` on staged `.cs` files
- `agent-docs/samples/pre-push` — runs `dotnet build` + `dotnet test --filter TestCategory!=Manual`

Before working, check if hooks are installed in `.git/hooks/`:

```ps
ls .git/hooks/pre-commit .git/hooks/pre-push
```

If missing, copy them from samples and make executable:

```ps
cp agent-docs/samples/pre-commit .git/hooks/pre-commit
cp agent-docs/samples/pre-push .git/hooks/pre-push
chmod +x .git/hooks/pre-commit .git/hooks/pre-push
```

On fresh clones, always run this setup step. Hooks outside `.git/` are not tracked by Git — `agent-docs/samples/` preserves them in version control.

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

## Logging

When adding debug/diagnostic logging to a class, always declare a static logger at the top of the class:

```csharp
// Instance class
internal class AutoEffectToken : CardTextToken<CardEffect>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<AutoEffectToken>();
    // ...
}

// Static class
public static class MultiClauseEffectParser
{
    private static readonly ILogger Log = Serilog.Log.ForContext(typeof(MultiClauseEffectParser));
    // ...
}
```

Use `Log.Debug(...)` for parsing diagnostics, token match traces, and intermediate state. This is the standard for all effect tokens and parser classes.

## Code Analysis

- Do not guess how code works — verify with debug logs.
- If a class lacks debug logs and you need to understand its behavior, add them before making changes.
- Log intermediate values, branch decisions, and unexpected states to visualize execution flow.

## Plans

Save implementation plans in `.opencode/plans/` by default. Use descriptive kebab-case filenames (e.g., `fix-csv-crosscheck-failures.md`). Plans are created before starting multi-step implementation work to outline approach and verify understanding.

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

If connected by a GitHub issue, also include `Fixes #<issue_number>` in the message body.

Examples for different AI tools:

**OpenCode:**
```
Fix deck exporter pipeline

Fixes #49

Extracted common export logic into base class.
Co-Authored-By: OpenCode <noreply@opencode.ai>
```

**Claude:**
```
Fix deck exporter pipeline

Fixes #49

Extracted common export logic into base class.
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Copilot:**
```
Fix deck exporter pipeline

Fixes #49

Extracted common export logic into base class.
Co-Authored-By: Copilot <noreply@github.com>
```

Use the appropriate `Co-Authored-By` line based on which AI tool assisted with the changes.

## Git Commit Pitfalls

- `git commit` silently succeeds doing nothing if no changes are staged. Always verify `git status` shows staged changes before committing.
- `commit.gpgsign=true` is set globally — use GPG signing by default. If GPG agent passphrase cache expires mid-session and `git commit` hangs waiting for input, fall back to `git commit --no-gpg-sign -m "msg"`. GPG key `<key_id>` is active — verify with `gpg --list-secret-keys` if signing issues arise.
- Multi-line commit messages (e.g. subject + body + `Co-Authored-By` trailer) cannot use `-m "line1\nline2"` — PowerShell does not pass embedded newlines through `-m` correctly. Use separate `-m` arguments for each paragraph:
  ```ps
  git commit -m "Subject line" -m "Body paragraph" -m 'Co-Authored-By: OpenCode <noreply@opencode.ai>'
  ```
  Each `-m` becomes a paragraph separated by a blank line. Verify with `git log -1 --format="%B"`.
- Angle brackets `<>` inside a `-m` argument (as in `Co-Authored-By: <email>`) cause PowerShell to interpret them as redirection operators in **double-quoted** strings (`"..."`). Use **single quotes** (`'...'`) for any `-m` value containing `<>`. Single quotes pass the content literally without any interpretation.

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

> **Design Flaw:** `EventCardEffect` does not implement `IConditionalCardEffect`, but recent changes give events built-in costs and condition-parsing via `MultiClauseEffectParser`. This means `EventCardEffect` should now implement `IConditionalCardEffect` so the translator service's unmatched-condition scan (in `WeissSchwarzCardTranslatorService.Translate`) can properly flag unresolved conditions in event text. Without it, the `?: []` branch silently ignores unmatched conditions for events.

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

## Translation Tokens

For guidelines specific to card text token parsing and translation, see [`Token/README.md`](Montage.Weiss.Tools/Entities/Effect/Token/README.md).

## Failed Translation Patch Workflow

When `TranslatePostProcessor` encounters unrecognized card text, it writes `./Export/failed_translation_report.json` and throws. To turn these failures into CSV test data, generate a **patch file** that maps each unmatched JP clause to its correct English translation.

### 1. Locate the report

```ps
ls ./Export/failed_translation_report.json
```

### 2. Read the report

Deserialize structure:

```
FailedTranslationReport
  └─ FailedTranslationEntry[]
       ├─ Serial          (card serial, e.g. "NIK/S117-001")
       └─ FailedAbilityEntry[]
            ├─ JapaneseText   (full JP effect text)
            └─ Tree           (partial CardEffect with unmachted clauses)
                 ├─ EffectText   (combined partial English + JP fragments)
                 ├─ Abilities[]
                 │    └─ UnmatchedAbility (when "$type":"Unmatched")
                 │         ├─ AbilityText   (JP fragment that wasn't matched)
                 │         └─ Suggestions[] (diagnostic hints like "ConditionEnding (Type.If)")
                 └─ Condition[] (when tree is ContCardEffect / AutoCardEffect)
                      └─ CardEffectCondition (when IsUnmatched: true)
                           └─ ConditionText  (JP fragment)
```

### 3. Determine the correct English for each unmatched clause

For each `UnmatchedAbility` (or unmatched `CardEffectCondition`):

- Read the `Suggestions` array — it contains hints like `"ConditionEnding (Type.If)"`, `"StartsWith: subject prefix"`, `"Multi-clause: 2 clauses"`.
- Cross-reference the JP fragment grammar pattern with existing tokens in:
  - `Montage.Weiss.Tools/Entities/Effect/Token/Ability/` — ability tokens
  - `Montage.Weiss.Tools/Entities/Effect/Token/Condition/` — condition tokens
- Look at similar cards in existing CSV files (`Montage.Weiss.Tools.Test/Resources/Translations/*.csv`) for reference English wording.
- Follow the `Token/README.md` conventions:
  - Condition text: do NOT include `"If"`, `"When"`, `"During"` prefixes — the framework prepends these from `ConditionType`.
  - Ability text: should be a full clause with proper casing (starts uppercase).
  - Labels: use the same values as existing CSV entries (e.g., `Assist`, `CXCOMBO`, `Brainstorm`).
  - **Names and Traits are NOT translated** — preserve Japanese names (`「」`) and traits (`《》`) as-is in the English output. Convert the Japanese corner brackets `「」` to regular ASCII double quotes, but do not convert special Unicode double quotes to ASCII double quotes (when in doubt, check the escape sequence as they should still be U+201C and U+201D, respectively). If an existing CSV has a translated name, change the CSV entry back to the JP name.

### 4. Write the patch JSON

```json
{
  "tokens": {
    "相手の前列のキャラを1枚選び、そのターン中、パワーを－6000": "choose 1 of your opponent's characters in their center stage, and those characters get -6000 power.",
    "jp_fragment_from_ability_or_condition": "correct english text"
  },
  "labels": {
    "【自】 このカードが手札から舞台に置かれた時、相手の前列のキャラを1枚選び、そのターン中、パワーを－6000。": ["CXCOMBO"]
  }
}
```

- `tokens` keys are **clause-level JP fragments** from `UnmatchedAbility.AbilityText` or `CardEffectCondition.ConditionText`. Multiple effects may reuse the same fragment.
- **Do not include trailing punctuation (period, comma) in `tokens` values.** The JP fragments in the report have already had their trailing punctuation stripped by the parser. Adding punctuation back in the patch value will duplicate it during `EffectText` reconstruction (e.g., `"text."` combined with a trailing `.` from the parent token → `"text.."`).
- `labels` keys are the **full JP effect text** from `FailedAbilityEntry.JapaneseText`. The value is the label array for that specific entry.
- **Omit entries with empty label arrays** — the CSV writer already produces an empty string for entries without labels. Explicitly setting `"effect": []` in the `labels` object is both unnecessary and risks overwriting labels that the partial tree correctly resolves.

### 5. Generate the CSV

```ps
dotnet run --project Montage.Weiss.Tools --no-build -- debug generate-csv --patch path/to/patch.json
```

### 6. Place in test resources

Move the CSV (default: `./Export/failed_translations_YYYYMMdd.csv`) into `Montage.Weiss.Tools.Test/Resources/Translations/`:
- `expansion_{setId}_effects.csv` for expansion sets
- `effects_{setId}.csv` for standalone sets (e.g., trial decks)

### 7. Verify rows exist (they are expected to fail)

```ps
dotnet test --filter "TestCategory~<SERIAL_PREFIX>" --list-tests
```