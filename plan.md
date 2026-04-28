## Plan: Add `./wstools update` Command

### Overview
Create a new command that updates existing cards in `cards.db` using specified post-processors, without re-parsing from URLs.

### Command Format
```
./wstools update rid1;rid2;rid3 postprocessoralias1;postprocessoralias2
```

### Implementation Steps

**1. ✅ Add `Alias` property to `ICardPostProcessor<C>` interface** (`Montage.Card.API/Interfaces/Services/ICardPostProcessor.cs`)
   - Added `string[] Alias { get; }` property

**2. ✅ Implement `Alias` in all post-processors**
   - `YuyuteiPostProcessor.cs`: `public string[] Alias => new[] { "yyt", "yuyutei" };`
   - `DeckLogPostProcessor.cs`: `public string[] Alias => new[] { "decklog" };`
   - `JKTCGPostProcessor.cs`: `public string[] Alias => new[] { "jktcg" };`
   - `DuplicateCardPostProcessor.cs`: `public string[] Alias => new[] { "duplicate" };`

**3. ✅ Modify `UpdateVerb.cs`** (`Montage.Weiss.Tools/CLI/UpdateVerb.cs`)
   - Added options for release IDs and post-processor aliases
   - Implemented update logic that queries cards by ReleaseID and applies post-processors
   - Skips `ISkippable<IParseInfo>` checks (as per issue requirement)
   - Keeps `IsCompatible()` checks (as per issue requirement)
   - Updates database with processed cards

**4. Implement Update Logic in `Run()` method using DI**
   - Query cards from database by ReleaseID
   - Get post-processors from DI and filter by alias
   - Apply post-processors using `Aggregate` pattern
   - Update database (remove old, add updated)
   
   > **Note:** Yuyutei post-processor will skip cards when multiple Release IDs are specified (e.g., `W53;WE27`) because `YuyuteiPostProcessor.IsCompatible()` returns `false` when `cards.Select(c => c.ReleaseID).Distinct().Count() > 1`. This is a known limitation that will be addressed in a future version.

**5. ✅ Handle Database Update**
   - Remove old cards by ReleaseID
   - Add processed cards with updated `VersionTimestamp`

**6. ✅ Add Tests**
   - Created `Montage.Weiss.Tools.Test/Internal/UpdateVerbTests.cs`
   - Tests verify Alias property on all post-processors
   - Tests verify all post-processors implement Alias property

### Key Design Decisions
- **Skip `ISkippable` checks**: Don't apply `skip:yyt` etc. logic (issue says "skip flag checks")
- **Keep `IsCompatible` checks**: Issue says "will not skip the compatibility phase"
- **DI-based alias lookup**: No static dictionary - use `pp.Alias` property from DI instances

### Files Modified
| File | Change |
|------|--------|
| `Montage.Card.API/Interfaces/Services/ICardPostProcessor.cs` | Added `Alias` property |
| `Montage.Weiss.Tools/Impls/PostProcessors/YuyuteiPostProcessor.cs` | Implemented `Alias` |
| `Montage.Weiss.Tools/Impls/PostProcessors/DeckLogPostProcessor.cs` | Implemented `Alias` |
| `Montage.Weiss.Tools/Impls/PostProcessors/JKTCGPostProcessor.cs` | Implemented `Alias` |
| `Montage.Weiss.Tools/Impls/PostProcessors/DuplicateCardPostProcessor.cs` | Implemented `Alias` |
| `Montage.Weiss.Tools/CLI/UpdateVerb.cs` | Main implementation |
| `Montage.Weiss.Tools.Test/Internal/UpdateVerbTests.cs` | Added tests |

### Verification
```ps
dotnet build
dotnet test --filter TestCategory!=Manual
```

### Status: ✅ Implementation Complete
- Build: Success (0 errors)
- Tests: All passed (7 new tests + existing tests)
- Command help: Verified working
- Regression fix: `./wstools update` without arguments now runs migration correctly
