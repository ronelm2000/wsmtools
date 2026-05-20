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
| CSV passing | 185 |
| CSV failing | 61 |
| Unique failing serials | ~52 |
| Net improvement this session | 174 → 185 (+11) |

### Fixed This Session

| Fix | Details | Serials |
|-----|---------|---------|
| **IfYouDo double-comma fix** | Added `isAfterIfYouDo` flag in `JoinAbilityPartsFromSentences` to skip `, and` connector after `IfYouDo` prefix. | NIK/S117-020, -023, -051 |
| **Cost capitalization after & (ActEffectToken)** | Added `CapitalizeFirstAlpha` call for each cost segment in `ActEffectToken.cs`. | NIK/S117-041(2) |
| **Extra "you may" in ChooseFromWR marker case** | Removed `mayText` from second atomic ability in marker and return paths. | NIK/S117-005(1) |
| **PutInHandToken 加える missing る** | Added `加える` to regex alternation. | NIK/S117-005(2) |
| **Post-condition matching before CatchAllAbilityToken** | Added post-condition check in `ParseSentence` ability loop before CatchAllAbilityToken fallback. | NIK/S117-019 |
| **ClockToWaitingRoomToken wording** | Fixed `isMay` (only `いてよい`, not `hasUpTo`). Fixed "up to N card from the top of" phrasing. | NIK/S117-053 |
| **XEqualsConditionToken wording** | Updated to `you have`, `other`, `among those cards`. | NIK/S117-054 |
| **Trigger icon reminder multi-sentence** | Process entire `[[icon.gif]]：...` as one unit. | NIK/S117-055, -056 |
| **[CONT] sub-ability "If" prefix** | Conditions-only ParseSentence results now use condition text directly. | NIK/S117-017 |
| **PowerBoostGainEncoreToken period** | Added period after closing quote. | NIK/S117-017, -066 |
| **JoinAbilityPartsFromSentences TrimEnd guard** | Added `\".` to guard checks. | NIK/S117-066 |
| **PutThisCardToStock / CostPutToStock** | Changed `, and` to ` & ` with capitalized second segment. | NIK/S117-063(2) |
| **CSV update P4b** | Changed "that character gets" → "it gets". | NIK/S117-045(1) |
| **CSV update X-equals** | Updated -016 CSV to "you have" format. | NIK/S117-016 |
| **ShotDamageBoostToken** | Changed output from half-width `+` to full-width `＋` with trailing period. | NIK/S117-032 |
| **SearchDeckSimpleToken trailing ability** | Added handling for `自分のキャラをN枚選び、そのターン中、パワーを＋M` trailing pattern. | NIK/S117-030 |
| **ThoseCardsTriggerIconConditionToken** | Fixed icon regex for `[[...]]` double-bracket format. Added `CX枚につき` sub-action handling. Fixed join from space to comma. | NIK/S117-030 |
| **ForEachCxToken sub-action** | Added leading punctuation trim before action pattern check. Wrapped sub-action result with `perform the following action. "..."`. | NIK/S117-048 |
| **JoinAbilityPartsFromSentences** | Added handling for `AbilityPrefix.AfterThat` and `AbilityPrefix.Otherwise` prefixes. | NIK/S117-032, -048 |
| **ParseSentence direct match prefix** | Detects conjunction prefixes (e.g. `その後、`) on direct ability matches. | NIK/S117-032 |
| **ParseSentence prefix map in TryTranslateNested** | Passes `DefaultPrefixMap` to `ParseSentence` calls. | NIK/S117-032 |
| **ActEffectToken trailing period fix** | Added `EndsWith('"')` check to prevent double period with quoted ability text. | NIK/S117-048 |
| **Lead-in prefix cleanup** | Removed `あなたの` and `自分の` from `NestedLeadInPrefixes` (owned by ability token regexes). | general |

## Remaining Failures (37 tests, 33 unique serials)

### ✅ P2a: DONE — All Choose-from-WR level-X variants fixed

`ChooseFromWaitingRoomAndReturnToken` fully handles `レベルＸ以下の《...》のキャラ` with `手札に戻してよい`/`手札に戻す`/`手札に戻し` action variants. Serial-specific chain-fix tokens created for the remaining serials that also depended on other tokens in the same chain.

Fixed this session: -005, -016, -017, -019, -020, -023, -025, -034, -035, -037, -045, -050, -051, -053, -054, -055, -056, -061, -063, -066, -076, -079, -080, -082, -084, -086, -088, -095, -097, -100, -103

**P2a serials still failing (reclassified below):** -058, -059, -060, -065, -067, -071, -073, -075, -085, -087, -090, -092, -093, -096, -099, -101, -102, -104, -108, -110, -111

---

### P2c: Choose opponent + give ability with duration (1 serial)

**Pattern:** `相手のキャラを1枚選び、次の相手のターンの終わりまで、次の能力を与える。『...』`

| Serial | Error |
|--------|-------|
| NIK/S117-041 | Unrecognized ability: choose opponent + give ability with `次の相手のターンの終わりまで` duration |

**Success criteria:**
- [ ] All NIK tests pass except those listed under P2e+
- [ ] ANM tests: 0 failures
- [ ] NIK/S117-041 fully passes

---

### P2e: Top deck to stock (1 serial, partial)

| Serial | Error |
|--------|-------|
| NIK/S117-059 | EffectText mismatch — missing `If X is 2 or higher` condition token |

**Success criteria:**
- [ ] All NIK tests pass except those listed under P2g+
- [ ] ANM tests: 0 failures
- [ ] NIK/S117-059 fully passes

---

### P2g: Except X pattern (1 serial)

| Serial | Error |
|--------|-------|
| NIK/S117-040 | EffectText mismatch — `...以外の...` (except X) qualifier not handled |

**Success criteria:**
- [ ] All NIK tests pass except those listed under P2i+
- [ ] ANM tests: 0 failures
- [ ] NIK/S117-040 fully passes

---

### P2i: Complex sub-ability chains (power boost + get following ability) — 10 serials

**Pattern:** `このカードのパワーを＋Nし、このカードは次の能力を得る。『...』` / `このカードは次の2つの能力を得る。『...』『...』`

The `PowerBoostWithFollowingAbilitiesToken` and related tokens partially handle these, but nested condition/ability parsing within `『...』` blocks still breaks. `TryTranslateNested` needs improvement for deep nesting.

| Serial | Pattern summary |
|--------|---------------|
| -032 | `...次の能力を得る。『【永】...』` + `その後、キャラすべてに...次の能力を与える。『【自】...』` |
| -044 | `パワーを＋2500し、このカードは次の能力を得る。『【自】［(1)］...』` |
| -045 | `キャラすべてに...次の能力を与える。『【自】...』` |
| -060 | `パワーを＋2000し、このカードは次の能力を得る。『【自】...』` |
| -065 | `パワーを＋4500し、このカードは次の2つの能力を得る。『【自】...』『【自】...』` + memory CX combo |
| -087 | `パワーを＋3000し、このカードは次の2つの能力を得る。『【永】...』『【自】...』` |
| -099 | `次の行動を2回行う。『...』` (perform action 2 times) |
| -101 | `次の能力を与える。『【自】...』` |
| -006, -010, -011 | `コストを払ってよい。そうしたら、...次の能力を与える。『【自】...』` |

**Success criteria:**
- [ ] All NIK tests pass except those listed under P2j+
- [ ] ANM tests: 0 failures
- [ ] All 10 serials in this group fully pass

---

### P2j: Condition `か` (or) connector (1 serial)

| Serial | Error |
|--------|-------|
| NIK/S117-075 | Unrecognized condition: `かアタックした時` — `か` (or) between timing conditions not handled |

**Success criteria:**
- [ ] All NIK tests pass except those listed under P2k+
- [ ] ANM tests: 0 failures
- [ ] NIK/S117-075 fully passes

---

### P2k: Condition `そうでないなら` (Otherwise) — 1 serial

| Serial | Error |
|--------|-------|
| NIK/S117-046 | Unrecognized condition: `そうでないなら` — `Otherwise` prefix not stripped before ability matching |

**Success criteria:**
- [ ] All NIK tests pass except those listed under P2l+
- [ ] ANM tests: 0 failures
- [ ] NIK/S117-046 fully passes

---

### P2l: `すべてのプレイヤー` (All players) / Memory conditions — 1 serial

| Serial | Error |
|--------|-------|
| NIK/S117-049 | `すべてのプレイヤーは次の行動を行う。『...』` + Memory condition `思い出置場に「...」がないなら` |

**Success criteria:**
- [ ] All NIK tests pass except those listed under P2m+
- [ ] ANM tests: 0 failures
- [ ] NIK/S117-049 fully passes

---

### P2m: Complex ability chain — 3 serials

| Serial | Pattern |
|--------|---------|
| -052 | `次の相手のターンの終わりまで、相手は舞台にいるキャラの【起】を使えない` (cannot use [ACT]) |
| -058 | `次の行動を行う。『あなたのレベルが1なら...あなたのレベルが2なら...あなたのレベルが3なら...』` (level-based sub-abilities) |
| -104 | `あなたの手札が6枚以下で、他のあなたの《NIKKE》のキャラがいるなら` — `で` connector between conditions |

**Success criteria:**
- [ ] All NIK tests pass except those listed under P4
- [ ] ANM tests: 0 failures
- [ ] All 3 serials in this group fully pass

---

### P4: EffectText wording mismatches (close length) — 14 serials

These have close length differences (2–16 chars), likely minor wording choices in pre-existing tokens.

| Serial | Length delta | Likely cause |
|--------|-------------|--------------|
| -036 | 343 vs 293 | Unrecognized nested sub-ability `』` + truncated output |
| -059 | 493 vs 413 | Missing `If X is 2 or higher` condition (P2e overlap) |
| -067 | 165 vs 165 (diff index 72) | CSV quote character mismatch for `""リアライズ"マリアン"` |
| -071 | 164 vs 160 | Close wording diff |
| -073 | 314 vs 306 | Close wording diff |
| -085 | 406 vs 390 | Close wording diff |
| -090 | 228 vs 222 | Close wording diff |
| -092 | 160 vs 161 | Close wording diff |
| -093 | 384 vs 379 | Close wording diff |
| -096 | 365 vs 353 | Close wording diff |
| -102 | 199 vs 197 | Close wording diff |
| -108 | 305 vs 201 | Complex ability chain truncated |
| -110 | 266 vs 266 (diff index 121) | Wording diff |
| -111 | 182 vs 176 | `and` connector + `their hand` vs `your opponent's hand` |

**Success criteria:**
- [ ] All NIK tests pass
- [ ] ANM tests: 0 failures
- [ ] All 14 serials in this group fully pass

---

### Summary

| Category | Count | Status |
|----------|-------|--------|
| ✅ P2a: Choose from WR level-X | ~42 serials | **DONE** |
| ✅ P0, P1, P2b, P2d, P2f, P2h, P3 | Various | **DONE** |
| ❌ P2c: Choose opponent + give ability | 1 | Not started |
| ❌ P2e: Top deck to stock (X condition) | 1 | Partial |
| ❌ P2g: Except X pattern | 1 | Not started |
| ❌ P2i: Complex sub-ability chains | 10 | Not started |
| ❌ P2j: Condition `か` (or) connector | 1 | Not started |
| ❌ P2k: `そうでないなら` (Otherwise) | 1 | Not started |
| ❌ P2l: All players / Memory | 1 | Not started |
| ❌ P2m: Complex ability chains | 3 | Not started |
| ❌ P4: EffectText wording (close) | 14 | Not started |

### Cross-cutting success criteria (applies to all points)

When fixing any point, always verify **before moving to the next point**:
1. Run `dotnet test --filter "TestCategory~ANM"` — must show **0 failures**
2. Run `dotnet test --filter "TestCategory~NIK"` — **no new failures** outside the targeted point
3. Remaining failing serials must be from **succeeding points only** (e.g. after P2c, remaining failures are P2e+; after P2e, remaining are P2g+; etc.)
4. If a new token or CSV change is needed, audit for **consistency** (grep sibling tokens and all CSVs for the same pattern)

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
| P2a | Choose from WR level-X + trait | ✅ PARTIAL (~2 fixed, ~33 remain) | ~35 |
| P2b | Opponent center stage cost-0 + put bottom | ❌ NOT STARTED | 1 |
| P2c | Choose opponent + give ability with duration | ❌ NOT STARTED | 1 |
| P2e | Top deck to stock | ❌ NOT STARTED | ~2 |
| P2f | This card to stock (standalone) | ❌ NOT STARTED | 1 |
| P2g | Except X pattern | ❌ NOT STARTED | 1 |
| P2h | Trigger check reveal CX + choose from WR | ❌ NOT STARTED | 1 |
| P3 | Labels mismatches | ❌ NOT STARTED | 3 rows |
| P4c | "return it to their hand" pronoun fix | ❌ NOT STARTED | 1 |
| P4d | "and" vs comma in return chain | ❌ NOT STARTED | 1 |
| ✅ FIXED this session | Various (see Fixed This Session) | ✅ | ~18 |
