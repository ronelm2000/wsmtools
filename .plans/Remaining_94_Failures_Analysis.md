# Remaining Failures Analysis

## Current Status
- **CSV test data**: `Resources/Translations/*.csv` (2 files, 246 rows)
- **CSV passing**: 164 (was 152)
- **CSV failing**: 82 (was 94)
- **Net improvement**: +12 tests passing
- **Unit tests (CI)**: 165 passed, 83 failed (246 CSV + 2 Assist = 248 total)

## Atomic Ability Decomposition

### Pattern Applied

`SearchDeckLevelAndCostToken` and 4 sibling tokens now emit atomic `CardEffectAbility` items instead of compound strings. The parent `AutoEffectToken.JoinAbilityPartsFromSentences()` joins them with ", " and ", and ".

### Decomposed Tokens

| Token | Atomic abilities | Items |
|-------|-----------------|-------|
| `SearchDeckLevelAndCostToken` | search → place as [REST] → shuffle | 3 |
| `SearchDeckFromLookToken` | search → reveal → (put to hand) → (shuffle) | 2–4 |
| `SearchDeckLevelCostAndPlaceToken` | search → place → shuffle | 3 |
| `SearchDeckForCxAndExchangeToken` | search → reveal → choose in hand → reveal → exchange → shuffle | 6 |
| `MoveToOpenPositionAndPutOpponentCharactersToWrToken` | move → put all chars to WR → gain trait | 3 |

### Tokens NOT decomposed

Tokens with conditional dependencies (inner "if" blocks) kept as single compound:
- `RevealTopCardAndIfTraitAddToHandToken`
- `RevealTopCardAndIfTraitAddToHandAndDiscardToken`
- `RevealTopCardAndIfEventOrTraitAddToHandToken`

See `Entities/Effect/Token/README.md` "Atomic Ability Pattern" for the full guideline.

## New Condition Tokens (14 created, 1 removed)

| File | Matches | Status |
|------|---------|--------|
| `RevealedCardTypeConditionToken` | `そのカードが《NIKKE》のキャラなら` | active |
| `RevealedCardLevelConditionToken` | `そのカードのレベルがN以上/以下なら` | active |
| `RevealedCardIsEventOrTraitConditionToken` | `そのカードが《NIKKE》のキャラかイベントなら` | active |
| `DeckCountConditionToken` | `山札がN枚以下なら` | active |
| `CardLevelConditionToken` | `そのカードがレベルN以上/以下のキャラなら` | active |
| `LevelThresholdConditionToken` | `レベルがN以上/以下なら` | active |
| `CxAreaHasCxConditionToken` | `CX置場にCXがあり` | active |
| `CxAreaCxWithIconConditionToken` | `CX置場にトリガーアイコンが[[...]]のCXがあり` | active |
| `CxAreaNamedConditionToken` | `あなたのCX置場に「name」があり` | active |
| `MultiConditionAndConnectiveToken` | `で、` (condition chain connector) | active |
| `YourCharacterReverseConditionToken` | `あなたのキャラが【リバース】した時` | active |
| `YourReverseCharactersCountConditionToken` | `あなたの【リバース】しているキャラがN枚以上なら` | active |
| `YourLevelOrLowerConditionToken` | `自分のレベル以下のレベル` | active |
| ~~`CxExistsConditionToken`~~ | ~~`あなたのCX置場にCXがあるなら`~~ | **removed** — dead code, shadowed by `CxWithTriggerIconInCxAreaConditionToken` (line 38) |

## New Ability Tokens (9 created)

| File | Pattern |
|------|---------|
| `CostPutTraitCharactersFromStageToWrToken` | Cost: Put N trait chars from stage to WR |
| `MoveToOpenPositionAndPutOpponentCharactersToWrToken` | Move + WR all opponent chars |
| `RevealTopCardAndIfEventOrTraitAddToHandToken` | Reveal + if event/trait add to hand |
| `RevealTopCardAndIfTraitAddToHandAndDiscardToken` | Reveal + if trait add + discard |
| `RevealTopCardAndIfTraitAddToHandToken` | Reveal + if trait add to hand |
| `SearchDeckForCxAndExchangeToken` | Search CX + exchange with hand |
| `SearchDeckFromLookToken` | `山札を見て...` (prefix-stripped variant) |
| `SearchDeckLevelAndCostToken` | Search deck with level/cost constraint |
| `SearchDeckLevelCostAndPlaceToken` | Search + place on stage |

## Bug Fixes & Consistency Audits

| Fix | Files changed | Impact |
|-----|--------------|--------|
| **Cost capitalization after `&`** | `AutoEffectToken.cs` | Each cost segment after `&` now capitalized |
| **Pluralization** | `ChooseCharacterAndBoostToken`, `ChooseTraitCharacterAndPowerBoostToken` | `"that character gets"` / `"those characters get"` |
| **"If You" → "if you"** | `AnotherSpecificCardNotExistsConditionToken` | Lowercase `you` in condition text |
| **"opponent's" → "your opponent's"** | `LookAtTopCardsToken` | `"your opponent's deck"` |
| **"X" preservation** | `AutoEffectToken.cs` (`JoinAbilityPartsFromSentences`) | Don't lowercase `X` variable names |
| **"there is a CX" wording** | `CxWithTriggerIconInCxAreaConditionToken`, `CxAreaHasCxConditionToken` | Both use `"there is a CX"` consistently |
| **"If you do" fallback** | `MayPayCostThenAbilityToken` | Raw text fallback when `ParseSentence` returns no abilities |
| **`×` glyph consistency** | `AssistPowerBoostToken`, `TranslatorServiceTests.cs`, `effects_550.csv` | Reverted `x`→`×` (code), updated 2 unit tests and 1 CSV to match |
| **CSV expected value** | `effects_550.csv:5` (ANM/W138-T07) | Changed `"this card's damage"`→`"damage dealt by this card"` to match `DamageCanceledConditionToken` |

## Infrastructure Changes

| Change | Details |
|--------|---------|
| **CSV directory move** | `Resources/*.csv` → `Resources/Translations/*.csv`. Test code uses `Directory.GetFiles` with `*.csv` glob instead of hardcoded list |
| **README audit docs** | Added `"When Modifying an Existing Token's Output"` section with cross-referencing checklist |
| **Dead code removed** | `CxExistsConditionToken` deleted — its regex is a subset of `CxWithTriggerIconInCxAreaConditionToken` (registered earlier) |

## Remaining Root Causes

### Priority 1: "If you do, ." bug (RC-AD) — ~12 rows
Partially fixed by the `MayPayCostThenAbilityToken` fallback. Still failing when cost/condition structures prevent `MayPayCostThenAbilityToken` from matching.

### Priority 2: Post-timing truncation (RC-AD) — ~15 rows
After timing conditions, action text is lost because ability tokens don't match remaining patterns.

### Priority 3: Sub-ability Grant — ~10 rows
Inner quoted abilities in `『...』` blocks not fully translated.

### Priority 4: Token Regex Bugs — ~5 rows
Remaining Japanese text in output from uncovered patterns (card-type conditions, multi-condition chains with `で、`, CX area checks with specific icons).

### Priority 5: Output Format (RC-AE) — ~40 rows
Minor wording differences: `"the turn this card was placed on stage from the hand"` vs `"the turn that this card is placed on the stage in your hand"`, `"and"` vs `"if"` in condition chains, `"it gets"` vs `"that character gets"` in some choose+boost patterns (CSVs use `"it gets"` for `ChooseTraitCharacterAndPowerBoostToken` output while token now uses `"that character gets"`).
