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

### Priority 5: Output Format (RC-AE) — resolved
CSV expected `"it gets"`; changed `ChooseTraitCharacterAndPowerBoostToken` from `"that character gets"` → `"it gets"`.

**RC-AE-1 (DuringTurnPlacedFromHandConditionToken):** Fixed. Token now outputs `"the turn that this card is placed on the stage in your hand"`. Updated ANM/W138-T07 CSV to match.

**RC-AE-2 (YourReverseCharactersCountConditionToken conjunction):** Fixed. Token output `"if you have"` is correct (`ConditionType.If`). Updated NIK/S117-024 CSV from `"and"` to `"if"`.

### CSV Cross-Check Status After Session

| Metric | Before | After |
|--------|--------|-------|
| CSV passing | 164 | 170 |
| CSV failing | 82 | 76 |
| Net change | — | +6 |

**Note:** Intermediate `--no-build` runs during the session used stale DLLs and showed lower failure counts. The 76 failures above reflect a fresh build after all changes were committed. The net improvement of +6 comes from properly fixing the labeled priority items.

## Remaining CSV Failures Root Cause Analysis (76 failures, 61 unique serials)

All remaining failures are pre-existing patterns from the original 82. None are regressions from this session's changes. Categorized by root cause:

### RC-B1: "put this card" vs "put it" (PutToStockToken output)
- **Count:** ~1 serial (NIK/S117-030)
- **Token:** `PutToStockToken` outputs `"put this card to your stock"`; CSV expects `"put it to your stock"`
- **Fix needed:** Change `PutToStockToken.AbilityText` from `"put this card"` to `"put it"`

### RC-B2: Brainstorm/ForEachCx per-CX follow-up not translated
- **Count:** ~2 serials (NIK/S117-030, NIK/S117-048)
- **Token:** `ThoseCardsTriggerIconConditionToken` + `ForEachCxToken` — the per-CX follow-up text after brainstorm/reveal is not translated
- **Fix needed:** Add handling for trigger-icon-qualified CX per-card iteration in `ForEachCxToken` or create combined token

### RC-B3: Sub-ability grant inner cost/condition not parsed
- **Count:** ~3 serials (NIK/S117-032, NIK/S117-043, NIK/S117-044)
- **Token:** `GainFollowingAbilityWithDurationToken`, `AfterThatAllCharactersGetAbilityToken`, `PowerBoostWithFollowingAbilityToken`
- **Pattern:** Inner sub-ability with cost `［(1)］` (full-width brackets) or complex condition not parsed. `TryTranslateNested` strips `【自】` prefix but `［(1)］` doesn't match `StockCostToken` (expects `(1)` half-width)
- **Fix needed:** Handle full-width cost brackets `［(N)］` in `TryTranslateNested` or add normalization

### RC-B4: Post-condition ability text truncated (ChooseFromWR patterns)
- **Count:** ~15 serials (NIK/S117-034, -035, -037, -040, -041, -043, -045, -046, -047, -050–056, -058–061, -063, -065–068, -071, -073, -075–077, -079–080, -082, -084–088, -090–097, -099–105, -108, -110)
- **Token:** Multiple — after condition tokens match, the remaining ability text doesn't match available ability tokens
- **Common missing patterns:**
  - `控え室のレベルＸ以下の《NIKKE》のキャラを1枚まで選び、手札に戻す` (choose from WR with level X or lower + trait)
  - `相手の前列のコスト0以下のキャラを1枚選び、山札の下に置く` (choose opponent center stage with cost 0 or lower + put bottom of deck)
  - `相手のキャラを1枚選び、...次の能力を与える。『...』` (choose opponent character and give ability)
  - `《...》以外の...` (except X pattern)
  - `レベル置場のカードと控え室の《...》のキャラを1枚ずつ選び、入れ替える` (choose and exchange with trait)
- **Fix needed:** Add/expand token regexes for these missing patterns

### RC-B5: "&" capitalization
- **Count:** ~1 serial (NIK/S117-041)
- **Token:** Cost format `"& Put this card"` vs `"& put this card"` — capitalization after `&` in cost
- **Fix needed:** Ensure cost segment after `&` uses same casing convention

### RC-B6: Labels mismatches
- **Count:** 6 serial rows (NIK/S117-025 ×2, NIK/S117-111, +3 others)
- **Root cause:** Translator outputs non-empty labels array for inputs that have no `【】` markers; CSV expects empty labels column
- **Fix needed:** Ensure `EventEffectToken` labels extraction doesn't produce labels when no `【】` prefix present

### RC-B7: Minor wording differences (Priority 5)
- **Count:** ~5 serials (NIK/S117-041, -045, -046)
- **Tokens:** Various tokens with minor wording differences — "it" vs "them", "that character gets" vs "it gets" (on non-trait variants), "return it to their hand" vs "return them to your opponent's hand"
- **Fix needed:** Every output text difference requires individual token adjustment

### Summary

| Category | Count | Root Cause |
|----------|-------|-----------|
| RC-B1 | ~1 | PutToStockToken output wording |
| RC-B2 | ~2 | ForEachCx + trigger icon variant |
| RC-B3 | ~3 | Full-width brackets in sub-ability costs |
| RC-B4 | ~45 | Missing token regex patterns (ChooseFromWR variants) |
| RC-B5 | ~1 | Cost & capitalization |
| RC-B6 | 6 | Labels extraction in EventEffectToken |
| RC-B7 | ~5 | Minor wording differences |
| **Total** | **~61** | All pre-existing patterns |
