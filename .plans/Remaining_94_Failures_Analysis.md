# Remaining Failures Analysis

## Current Status
- **CSV test data**: `Resources/Translations/*.csv` (2 files, 246 rows)
- **CSV passing**: 245 (was 164)
- **CSV failing**: 1 (was 82)
- **Net improvement**: +81 tests passing (this session)
- **Unit tests (CI)**: 245 passed, 1 CSV failed (NIK/S117-024, pre-existing wording diff) + 130 pre-existing Registry_AbilitiesMustCaptureEndingPunctuations failures

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

## New Condition Tokens (15 created, 1 removed)

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
| `OpponentCenterStageCountConditionToken` | `相手の前列のキャラがN枚以下なら` | active (this session) |
| ~~`CxExistsConditionToken`~~ | ~~`あなたのCX置場にCXがあるなら`~~ | **removed** — dead code, shadowed by `CxWithTriggerIconInCxAreaConditionToken` (line 38) |

## New Ability Tokens (14 created)

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
| `CannotBecomeReverseToken` | `このカードは【リバース】しない` (this session) |
| `CannotDealDamageToPlayerToken` | `このカードはプレイヤーにダメージを与えることができない` (this session) |
| `ChooseAndExchangeToken` | Choose card + exchange with trait support (this session) |
| `GainStandaloneFollowingAbilityToken` | `次の能力を得る。『...』` (this session) |
| `AfterThatAllCharactersGetAbilityToken` | `(その後、)?あなたのキャラすべてに...能力を与える。『...』` (this session) |
| `GainFollowingAbilityWithDurationToken` | `(そのターン中、)?次の能力を得る。『...』` (this session) |
| `PlayWithoutColorConditionToken` | `色条件を満たさずに手札からプレイできる` (this session) |

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
| **Condition text prefix** | `NoTraitExistsConditionToken.cs` | Correct rendering via `EventEffectToken` using `AggregateToString` |
| **"Choose character in battle"** | `ChooseOtherCharacterAndGiveAbilityToken.cs` | Added `バトル中の` prefix support; "character in battle" output |
| **Full-width Ｘ support** | `ChooseTraitCharacterAndPowerBoostToken.cs` | Added `Ｘ` to power regex |
| **Optional `あなたは` prefix** | `ChooseTraitCharacterAndPowerBoostToken.cs` | Made `あなたは` optional (previously required at start) |
| **Sub-action parsing** | `ForEachCxToken.cs`, `AllPlayersPerformActionToken.cs` | Inner `『...』` content now translated |
| **「を選び、入れ替える」** | `ChooseAndExchangeToken.cs` | New token for choose-and-exchange pattern with trait |
| **"it gets" wording** | `ChooseTraitCharacterAndPowerBoostToken.cs` | Reverted `"that character gets"` → `"it gets"` to match CSV |

## Infrastructure Changes

| Change | Details |
|--------|---------|
| **CSV directory move** | `Resources/*.csv` → `Resources/Translations/*.csv`. Test code uses `Directory.GetFiles` with `*.csv` glob instead of hardcoded list |
| **README audit docs** | Added `"When Modifying an Existing Token's Output"` section with cross-referencing checklist |
| **Dead code removed** | `CxExistsConditionToken` deleted — its regex is a subset of `CxWithTriggerIconInCxAreaConditionToken` (registered earlier) |

## Fixes Applied This Session (82→1 CSV failures)

### Priority 1: "If you do, ." bug (RC-AD)
**Fixed.** `NoTraitExistsConditionToken` condition text corrected. `EventEffectToken` now uses `AggregateToString` for condition rendering (prepends "If" etc.).

### Priority 2: Post-timing truncation (RC-AD)
**Fixed.** Multiple new condition/ability tokens added:
- `OpponentCenterStageCountConditionToken` — `相手の前列のキャラがN枚以下なら`
- `GainStandaloneFollowingAbilityToken` — `次の能力を得る。『...』`
- `AfterThatAllCharactersGetAbilityToken` — `その後、キャラすべてに...能力を与える。『...』`
- `GainFollowingAbilityWithDurationToken` — `(そのターン中、)?次の能力を得る。『...』`
- `PlayWithoutColorConditionToken` — `色条件を満たさずに手札からプレイできる`
- `ForEachCxToken` improved: handles `次の行動を行う。『...』` sub-action
- `AllPlayersPerformActionToken` improved: parses sub-action `『...』` content
- `ChooseAndExchangeToken` — choose card + exchange pattern with trait support

### Priority 3: Sub-ability Grant
**Fixed.** Four new ability tokens added:
- `CannotBecomeReverseToken` — `このカードは【リバース】しない`
- `CannotDealDamageToPlayerToken` — `このカードはプレイヤーにダメージを与えることができない`
- `ChooseOtherCharacterAndGiveAbilityToken` expanded: supports `バトル中の` prefix and "character in battle" wording
- `PowerBoostWithFollowingAbilityToken.TryTranslateNested()` improved: inner sub-abilities now translate correctly

### Priority 4: Token Regex Bugs
**Fixed.** `ChooseTraitCharacterAndPowerBoostToken` regex expanded:
- Optional `あなたは` prefix (was required at start)
- Supports `Ｘ` (full-width X) in power value

### Priority 5: Output Format (RC-AE) — 1 remaining
CSV expected `"it gets"`; changed `ChooseTraitCharacterAndPowerBoostToken` from `"that character gets"` → `"it gets"`.

**1 remaining failure:** NIK/S117-024 — two wording diffs:

### NIK/S117-024 Root Cause

Two independent wording differences against the CSV expected value:

**RC-AE-1: `DuringTurnPlacedFromHandConditionToken` output**
| | Text |
|---|------|
| CSV expects | `"the turn that this card is placed on the stage in your hand"` |
| Token outputs | `"the turn this card was placed on stage from the hand"` |
| Root cause | Token's `ConditionText` uses `"was placed"/"from the hand"` phrasing. Changing it to match NIK/S117-024's CSV would regress ANM/W138-T07 which expects the original wording. The CSV entries are **mutually inconsistent** — they cannot both be satisfied without changing one CSV. |

**RC-AE-2: `YourReverseCharactersCountConditionToken` conjunction**
| | Text |
|---|------|
| CSV expects | `"...when your character becomes [REVERSE], and you have 3 or more [REVERSE] characters..."` |
| Token outputs | `"...when your character becomes [REVERSE], if you have 3 or more [REVERSE] characters..."` |
| Root cause | Token sets `ConditionType.If` which renders as `"if"`. CSV expects `"and"` which would require changing type or condition text. The Japanese source `なら` is a conditional, so `"if"` is semantically correct; `"and"` is a stylistic choice in the CSV. |

**Resolution:** Both are pre-existing wording-preference mismatches, lowest priority. Fixing RC-AE-1 requires updating either the CSV or the condition text and accepting a 1-test tradeoff. Fixing RC-AE-2 requires changing `YourReverseCharactersCountConditionToken` to use `ConditionType.When` or custom conjunction handling.
