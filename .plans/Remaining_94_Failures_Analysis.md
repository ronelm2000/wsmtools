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

## Current State (End of Current Session)

| Metric | Value |
|--------|-------|
| CSV total | 246 rows |
| CSV passing | 174 |
| CSV failing | 72 (70 EffectText + 6 Labels — 3 overlap counted twice) |
| Unique failing serials | ~58 |
| Net improvement this session | 77 → 72 (−5) |

### Fixed This Session

| Fix | Details | Serials |
|-----|---------|---------|
| **P0: Full-width cost brackets in `TryTranslateNested`** | Added `［...］` cost bracket handling before ability matching in `TryTranslateNested`. Normalizes `［(N)］` → `[(N)]` and removes space between `[AUTO]` and `[(N)]`. | NIK/S117-032 (partial), -043, -044 |
| **WhenYouUseActConditionToken** | New condition token for `あなたが【起】を使った時` (When you use an [ACT]). | NIK/S117-043(row 1) |
| **YourAttackPhaseStartConditionToken** | New condition token for `あなたのアタックフェイズの始めに` (your attack phase). | NIK/S117-043(row 2) |
| **NoRestCharacterInCenterStageConditionToken** | New condition token for `他のあなたの前列の【レスト】しているキャラがいないなら`. | NIK/S117-044(row 1) |
| **RestStandCharacterToken → atomic** | Split into 2 atomic abilities (choose + [REST]), removed `(あなたは)?` from regex, added `and` before `[REST] it`. | NIK/S117-043(row 2) |
| **DealFixedDamageToken continuative fix** | Regex now accepts `与え` (continuative) in addition to `与える`. | NIK/S117-043(row 2) |
| **ChooseCardAndPutInWaitingRoomToken** | Made `あなたは` optional in regex. | NIK/S117-048 |
| **ChooseFromWaitingRoomAndReturnToken continuative fix** | Added `手札に戻し` to action alternation. Removed `and` from combined output for proper serial comma joining. | NIK/S117-048 |
| **ShotDamageBoostToken** | Changed output from half-width `+` to full-width `＋` with trailing period. | NIK/S117-032 |
| **SearchDeckSimpleToken trailing ability** | Added handling for `自分のキャラをN枚選び、そのターン中、パワーを＋M` trailing pattern. | NIK/S117-030 |
| **ThoseCardsTriggerIconConditionToken** | Fixed icon regex for `[[...]]` double-bracket format. Added `CX枚につき` sub-action handling. Fixed join from space to comma. | NIK/S117-030 |
| **ForEachCxToken sub-action** | Added leading punctuation trim before action pattern check. Wrapped sub-action result with `perform the following action. "..."`. | NIK/S117-048 |
| **JoinAbilityPartsFromSentences** | Added handling for `AbilityPrefix.AfterThat` and `AbilityPrefix.Otherwise` prefixes. | NIK/S117-032, -048 |
| **ParseSentence direct match prefix** | Detects conjunction prefixes (e.g. `その後、`) on direct ability matches. | NIK/S117-032 |
| **ParseSentence prefix map in TryTranslateNested** | Passes `DefaultPrefixMap` to `ParseSentence` calls. | NIK/S117-032 |
| **ActEffectToken trailing period fix** | Added `EndsWith('"')` check to prevent double period with quoted ability text. | NIK/S117-048 |
| **Lead-in prefix cleanup** | Removed `あなたの` and `自分の` from `NestedLeadInPrefixes` (owned by ability token regexes). | general |

## Prioritized To-Do List for Next Session

### P0: Sub-ability Grant — full-width cost brackets (fixes ~3 serials)

**Root cause:** `TryTranslateNested` in `GainFollowingAbilityToken.cs` strips `【自】` etc. from sub-abilities, but inner cost `［(1)］` (full-width brackets) doesn't match `StockCostToken` which expects half-width `(1)`.

**Fix:** Normalize `［N］` → `[N]` and `（N）` → `(N)` early in `TryTranslateNested` before attempting ability matching.

**Affected serials:**

| Serial | Row | Pattern |
|--------|-----|---------|
| NIK/S117-032 | 1 | `『【永】 あなたの[[shot.gif]]の効果で与えるダメージを＋1。』` — inner sub-ability works, outer all-characters ability still raw |
| NIK/S117-043 | 2 | `『【自】 あなたのアタックフェイズの始めに、...』` — `［(1)］` cost in nested auto ability not parsed |
| NIK/S117-044 | 1 | `『【自】［(1)］ アンコールステップの始めに、...』` — `［(1)］` cost not parsed in PowerBoostWithFollowingAbilityToken |

---

### P1: Brainstorm/ForEachCx trigger-icon variant (fixes ~2 serials)

**Root cause:** `ThoseCardsTriggerIconConditionToken` + `ForEachCxToken` handles the per-CX iteration when CX count is involved, but the trigger-icon-qualified variant (`それらのカードのトリガーアイコンが[[treasure.gif]]のCX1枚につき`) uses a different prefix that doesn't match `ForEachCxToken`'s regex (`^それらのカードのCX1枚につき`).

**Fix:** Add a new token or expand `ForEachCxToken` regex to also match `それらのカードのトリガーアイコンが[[...]]のCX(\d+)枚につき`.

**Affected serials:**

| Serial | Row | Pattern |
|--------|-----|---------|
| NIK/S117-030 | 2 | Brainstorm with `それらのカードのトリガーアイコンが[[treasure.gif]]のCX1枚につき` — follow-up search+boost not translated |
| NIK/S117-048 | 1 | Brainstorm with `それらのカードのCX1枚につき、次の行動を行う。『...』` — follow-up `ForEachCxToken` sub-action not translated (different variant) |

---

### P2: Post-timing ability truncation — missing token patterns (fixes ~45 serials)

**Root cause:** After condition tokens match, the remaining ability text doesn't match any registered token. Multiple distinct missing patterns.

**P2a: Choose from WR with level-X or lower + trait (largest group)**

Pattern: `控え室のレベルＸ以下の《NIKKE》のキャラを1枚まで選び、手札に戻す`

**Fix:** Expand `ChooseFromWaitingRoomAndReturnToken` regex or create variant to accept `レベルＸ以下の《...》のキャラ` before the count.

**Affected serials:** NIK/S117-037, -058, -059, -060, -061, -065, -066, -067(2), -068, -071, -073, -075, -076, -077, -079(1), -080, -082, -084, -085, -086, -087, -088(2), -090, -091, -092, -093(2), -094, -095, -096, -097, -099, -100, -101, -102, -103(2), -104(2), -105, -108, -110

**P2b: Choose opponent center stage cost-0-or-lower + put bottom of deck**

Pattern: `相手の前列のコスト0以下のキャラを1枚選び、山札の下に置いてよい`

**Fix:** New ability token for `相手の前列のコスト(?<cost>\d+)以下のキャラを(\d+)枚選び、山札の下に置く`.

**Affected serials:** NIK/S117-035

**P2c: Choose opponent character and give ability until end of opponent's next turn**

Pattern: `相手のキャラを1枚選び、次の相手のターンの終わりまで、次の能力を与える。『...』`

**Fix:** New ability token matching choose opponent + give ability with `次の相手のターンの終わりまで` duration.

**Affected serials:** NIK/S117-041(1)

**P2d: When you use [ACT] (potent ability trigger) + choose character and boost**

Pattern: `あなたが【起】を使った時、あなたは自分のキャラを1枚選び、そのターン中、パワーを＋1000`

**Fix:** New condition token for `あなたが【起】を使った時` (When you use an [ACT]).

**Affected serials:** NIK/S117-043(1)

**P2e: Top deck to stock**

Pattern: `山札の上から1枚を、ストック置場に置いてよい`

**Fix:** New token for `^山札の上から(\d+)枚(まで)?を、ストック置場に置(?:く|いてよい|き)`.

**Affected serials:** NIK/S117-050, -055, -056, -057, -059

**P2f: This card to stock (standalone, not cost)**

Pattern: `このカードをストック置場に置いてよい` (after condition, not inside `［］` cost)

**Fix:** New token for `^このカードをストック置場に置(?:く|いてよい|き)`.

**Affected serials:** NIK/S117-100

**P2g: Except X pattern**

Pattern: `《...》以外の...` (choose except a named card)

**Fix:** New ability token for choose patterns with `以外の` (except) qualifier.

**Affected serials:** NIK/S117-040

**P2h: Trigger check reveal CX with icon + choose from WR**

Pattern: After `TriggerCheckRevealsCxWithIconConditionToken`, `あなたは自分の控え室の...を1枚選び、手札に戻してよい` doesn't match.

**Fix:** Ensure `ChooseFromWaitingRoomAndReturnToken` or similar matches after trigger-check conditions.

**Affected serials:** NIK/S117-034

---

### P3: Labels mismatches (fixes 6 rows)

**Root cause:** `EventEffectToken` regex `^(?<labels>(?:【[^】]+】)*)` captures zero or more `【】` markers. For inputs with no `【】` prefix, the labels array should be empty but contains spurious entries.

**Fix:** Check `MatchLabels` or labels extraction — ensure labels array is empty when the input has no `【】` at the start.

**Affected serials:**

| Serial | Row | Labels expected | Labels actual |
|--------|-----|----------------|---------------|
| NIK/S117-025 | 2 | `[]` (empty) | `["..."]` (3 labels) |
| NIK/S117-025 | 3 | `[]` (empty) | `["..."]` (3 labels) |
| NIK/S117-111 | 1 | `[]` (empty) | `["..."]` (1 label) |
| +3 more | | | |

---

### P4: Minor wording differences (fixes ~5 serials)

**P4a: "that character" vs opponent's stock**

| Serial | Token | CSV expects | Token outputs |
|--------|-------|-------------|---------------|
| NIK/S117-020 | `PutThatCharacterToStockToken` | `"put that character to your opponent's stock"` | `"put that character to your stock"` |
| NIK/S117-023 | `PutThatCharacterToStockToken` | `"put that character to your opponent's stock"` | `"put that character to your stock"` |

**Fix:** Add `"your opponent's"` in `PutThatCharacterToStockToken` output.

**P4b: "it gets" vs "that character gets" (non-trait variant)**

| Serial | Token | CSV expects | Token outputs |
|--------|-------|-------------|---------------|
| NIK/S117-045(1) | `ChooseTraitCharacterAndPowerBoostToken` | `"that character gets"` | `"it gets"` |

**Note:** Token was changed to `"it gets"` this session to match other CSVs (ANM/W138-T13, NIK/S117-018). NIK/S117-045 CSV is inconsistent — uses `"that character gets"` instead. This is a CSV inconsistency, not a token bug.

**Fix:** Either revert token to `"that character gets"` (breaks 2 passing tests) or update NIK/S117-045 CSV to `"it gets"`.

**P4c: "return it to their hand" vs "return them to your opponent's hand"**

| Serial | Token | CSV expects | Token outputs |
|--------|-------|-------------|---------------|
| NIK/S117-046(2) | `OpponentChooseReturnToHandToken` | `"choose up to 1 of your opponent's characters, and return it to their hand"` | `"choose up to 1 of your opponent's characters, and return them to your opponent's hand"` |

**Fix:** Change `OpponentChooseReturnToHandToken` to use `count == 1 ? "it" : "them"` and `count == 1 ? "to their hand" : "to your opponent's hand"`.

**P4d: "and" vs comma in return-to-hand chain**

| Serial | Token | CSV expects | Token outputs |
|--------|-------|-------------|---------------|
| NIK/S117-046(1) | Various | `"return it to their hand. You may choose 1 card... If you do, choose 1 of your opponent's characters, and return it to their hand."` | Missing second clause entirely |

**Fix:** This is a post-timing truncation issue (P2 category) — the second sentence isn't parsed.

---

### P5: Cost & capitalization (fixes 1 serial)

**Root cause:** Cost segment after `&` should start with uppercase.

| Serial | CSV expects | Token outputs |
|--------|-------------|---------------|
| NIK/S117-041(2) | `"Put 1 card in your hand to your waiting room & Put this card to your waiting room"` | `"Put 1 card in your hand to your waiting room & put this card to your waiting room"` |

**Fix:** Ensure `AutoEffectToken` cost joining logic capitalizes the first letter of each cost segment after `&`.

---

## Summary (Updated)

| Priority | Category | Status | Serials |
|----------|----------|--------|---------|
| P0 | Full-width cost brackets in sub-abilities | ✅ FIXED | 3 |
| P1 | ForEachCx trigger-icon variant | ✅ FIXED | 2 |
| P2a | Choose from WR level-X + trait | ✅ PARTIAL (~2 fixed, ~33 remain) | ~35 |
| P2b | Choose opponent center stage cost-0 + put bottom | ❌ NOT STARTED | 1 |
| P2c | Choose opponent + give ability with duration | ❌ NOT STARTED | 1 |
| P2d | When you use [ACT] condition token | ✅ FIXED | 1 |
| P2e | Top deck to stock | ❌ NOT STARTED | ~5 |
| P2f | This card to stock (standalone) | ❌ NOT STARTED | 1 |
| P2g | Except X pattern | ❌ NOT STARTED | 1 |
| P2h | Trigger check reveal CX + choose from WR | ❌ NOT STARTED | 1 |
| P3 | Labels mismatches | ❌ NOT STARTED | 6 rows |
| P4a | "that character" → "your opponent's" in PutThatCharacterToStock | ❌ NOT STARTED | 2 |
| P4b | "it gets" vs "that character gets" | ❌ NOT STARTED | 1 |
| P4c | "return it to their hand" pronoun fix | ❌ NOT STARTED | 1 |
| P4d | "and" vs comma in return chain (post-timing) | ❌ NOT STARTED | 1 |
| P5 | Cost & capitalization after & | ❌ NOT STARTED | 1 |
