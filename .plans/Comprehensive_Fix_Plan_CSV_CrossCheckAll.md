# Comprehensive Fix Plan: Translate_CSV_CrossCheckAll Remaining Failures

## Summary
- **Total failing serials**: 97 unique serials (95 expansion_494 + 2 effects_550)
- **Total failing test rows**: 148
- **Pass rate**: 117/246 (+2 from 115 baseline)
- **Root causes identified**: 50+ distinct missing tokens/patterns (including newly identified)

## Session Progress (2026-05-18)

### Completed (Earlier Sessions)
- [x] **RC-AD-2/AD-6**: ActEffectToken refactored — replaced `GetMatch` cost parsing with `Match` loop; replaced `TryFindFirstMatch` ability loop with `MultiClauseEffectParser.Parse` for multi-sentence Brainstorm support
- [x] **Crash guard**: Added `parsed.Remaining` two-level check to `AutoEffectToken` and `ContEffectToken` (fixes `NotImplementedException` regressions)
- [x] **ContEffectToken token log**: Populated from parsed conditions/abilities (fixes `TokenLogMustNotBeEmpty`)
- [x] **Brainstorm "For each CX" token**: Created `ForEachCxToken` — handles `それらのカードのCX1枚につき、...` pattern without trigger icon (fixes NIK/S117-007, NIK/S117-048)
- [x] **Cost separator fix**: Both `AutoEffectToken` and `ActEffectToken` — stock cost `(N)` space-separated from next action, ` & ` between other cost parts
- [x] **ActEffectToken format**: Space before `[cost]` after labels (e.g., `Brainstorm [cost]` not `Brainstorm[cost]`); ability text capitalized after cost bracket

### Completed (This Session — Sprint 2)
- [x] **ConditionalPutInHandToken**: Made `る` optional in regex (`加える?`) — handles both continuative and dictionary forms
- [x] **SimplePowerBoostToken**: Added `(?:を)?` for `パワー(を)＋` variant and `[XＸ\d]` for variable X power boosts
- [x] **PowerBoostToken**: Added `[XＸ\d]` character class for `Ｘ` variable support
- [x] **CostPutCxFromHandToWaitingRoomToken**: Continuative form — regex now matches `置(?:く|き)`
- [x] **CostPutTraitCharacterFromHandToWaitingRoomToken**: Same continuative fix
- [x] **PutCardFromHandToWaitingRoomToken**: Added `、` to trailing punctuation alternation
- [x] **ChooseFromWaitingRoomAndReturnToDeckToken**: Made `まで` optional; singular/plural pronoun fix
- [x] **ChooseFromWaitingRoomAndReturnToken**: Added "you may " prefix when action contains `てよい`
- [x] **TokenRegistry.Match**: Added debug logging to trace which token matches at index 0
- [x] **DrawAndDiscardToken**: Created new token for `あなたはN枚引いてよい。そうしたら、あなたは自分の手札をN枚選び、控え室に置く。` pattern (+1 test)
- [x] **StrikerAbilityToken**: Created new token for `大活躍` keyword → `"Great Performance"` (+3 tests from NIK/S117-017)
- [x] **CannotBeChosenAbilityToken**: Created ability-level variant of `CannotBeChosenConditionToken` for nested `『』` contexts, produces `"This card cannot be chosen by your opponent's effects."`
- [x] **TryTranslateNested refactored**: Re-architected nested ability translation in `GainFollowingAbilityToken.cs`:
  - New order: lead-in stripping → effect match (with `【】` intact) → strip `【】` → ability match → `ParseSentence` fallback
  - Fixed `ContEffectToken` swallowing nested `【永】` text and producing `[CONT] If This card cannot be...` instead of `[CONT] This card cannot be...`
  - Added `TryMatchAbility`/`TryMatchEffect` split for proper registry order
- [x] **GainEncoreAbilityToken**: Added `[XＸ\d]` support in power boost sub-regex

### Test Results
| Metric | Before Sprint 2 | After Sprint 2 | Δ |
|--------|-----------------|----------------|---|
| CSV passes | 117 | **122** | **+5** |
| `NotImplementedException` | 29 | **24** | **-5** |
| Build errors | 0 | 0 | 0 |

---

## Root Cause Categories

### RC-A: Assist/Power Boost Tokens (High Impact - ~15 failures)

#### RC-A1: Assist Power Boost X = Level × N
**Pattern**: `このカードの前のあなたのキャラすべてに、パワーを＋Ｘ。Ｘはそのキャラのレベル×[N]に等しい。`
**Affected**: NIK/S117-015 (first row), NIK/S117-091 (first row)
**Fix**: `AssistPowerBoostToken` exists but regex may not match. Verify pattern.
**Status**: Token exists, needs regex fix.

#### RC-A2: "Per Other NIKKE Character" Power Boost
**Pattern**: `他のあなたの《NIKKE》のキャラ1枚につき、このカードのパワーを＋[N]。`
**Affected**: NIK/S117-078, NIK/S117-099
**Fix**: Create `PowerBoostPerOtherNikkeToken`.
**Status**: Missing.

#### RC-A3: "Per Opponent REST Character" Power Boost
**Pattern**: `相手の【レスト】しているキャラ1枚につき、このカードのパワーを＋[N]。`
**Affected**: NIK/S117-052 (first row)
**Fix**: Create `PowerBoostPerOpponentRestToken`.
**Status**: Missing.

#### RC-A4: Soul Boost +1
**Pattern**: `ソウルを＋1。`
**Affected**: NIK/S117-076 (first row)
**Fix**: Create `SoulBoostOneToken`.
**Status**: Missing.

---

### RC-B: Cost Tokens (High Impact - ~10 failures)

#### RC-B1: Put Trait Character from Hand to WR Cost
**Pattern**: `手札の《NIKKE》のキャラを1枚控え室に置く`
**Affected**: NIK/S117-044, NIK/S117-060 (second row), NIK/S117-069 (first row)
**Fix**: `CostPutTraitCharacterFromHandToWaitingRoomToken` exists but not matching. Check regex.
**Status**: Token exists, needs regex fix.

#### RC-B2: REST Stand NIKKE Character Cost
**Pattern**: `他のあなたの【スタンド】している《NIKKE》のキャラを1枚【レスト】する`
**Affected**: NIK/S117-108 (first row)
**Fix**: Create `CostRestStandNikkeCharacterToken`.
**Status**: Missing.

#### RC-B3: REST 2 NIKKE Characters Cost
**Pattern**: `あなたの《NIKKE》のキャラを2枚【レスト】する`
**Affected**: NIK/S117-068 (first row)
**Fix**: Create `CostRestTwoNikkeCharactersToken`.
**Status**: Missing.

#### RC-B4: Put Card to Stock & Swap Bottom Cost
**Pattern**: `このカードをストック置場に置き、あなたのストックの下から1枚を、控え室に置く`
**Affected**: NIK/S117-063 (second row)
**Fix**: Create `CostPutToStockAndSwapBottomToken`.
**Status**: Missing.

---

### RC-C: Encore/Ability Grant Tokens (Medium Impact - ~8 failures)

#### RC-C1: Gain Encore Ability
**Pattern**: `このカードは『【自】 アンコール ［手札の《NIKKE》のキャラを1枚控え室に置く］』を得る。`
**Affected**: NIK/S117-066 (first row), NIK/S117-093 (third row)
**Fix**: Create `GainEncoreAbilityToken`.
**Status**: Missing.

#### RC-C2: Give Encore to All Opponent Characters
**Pattern**: `相手のキャラすべてに、『【自】 アンコール ［(2)］』を与える。`
**Affected**: NIK/S117-105 (first row)
**Fix**: Create `GiveEncoreToAllOpponentCharactersToken`.
**Status**: Missing.

---

### RC-D: CXCOMBO/Memory Tokens (Medium Impact - ~12 failures)

#### RC-D1: CXCOMBO Named Card Placed Condition
**Pattern**: `CX置場に「[CardName]」が置かれた時` or `CX置場に「[CardName]」があり`
**Affected**: NIK/S117-030, 034, 035, 036, 037, 041, 043, 059, 066, 087, 093, 099, 104
**Fix**: `CxNamedPlacedConditionToken` exists. Check quote style handling (「」vs "").
**Status**: Token exists, needs quote style fix.

#### RC-D2: Memory Card Exists Condition
**Pattern**: `思い出置場にこのカードがあり`
**Affected**: NIK/S117-065 (second row)
**Fix**: Extend `NoCardInMemoryConditionToken` to handle positive form.
**Status**: Partial.

---

### RC-E: Trigger Icon Reminder Text (Medium Impact - ~15 failures)

#### RC-E1: Treasure Trigger Reminder
**Pattern**: `（[[treasure.gif]]：このカードがトリガーした時、...）`
**Affected**: NIK/S117-055, 056, 057, 081, 082, 083, 084
**Fix**: Add treasure icon to `TranslateTriggerIconReminderText` helper.
**Status**: Partial (shot, gate, salvage exist; treasure missing).

#### RC-E2: Choice/Standby/Gate/Shot Icons
**Pattern**: Various `[[icon.gif]]` in reminder text
**Affected**: Multiple serials
**Fix**: Complete all icon types in reminder text translator.
**Status**: Partial.

---

### RC-F: Level/Stage Placement Tokens (Medium Impact - ~10 failures)

#### RC-F1: Placed from Hand Power Boost
**Pattern**: `このカードが手札から舞台に置かれた時、そのターン中、このカードのパワーを＋X。`
**Affected**: NIK/S117-063 (first row), NIK/S117-071 (second row)
**Fix**: Create `PlacedFromHandPowerBoostToken`.
**Status**: Missing.

#### RC-F2: Look at Top Cards and Put in Any Order
**Pattern**: `山札を上から[N]枚見て、山札の上に好きな順番で置く`
**Affected**: NIK/S117-051 (first row), NIK/S117-072 (second row), NIK/S117-073 (first row)
**Fix**: `LookAtTopCardsToken` exists but may not handle follow-up action.
**Status**: Partial.

---

### RC-G: Battle/Reverse Tokens (Low Impact - ~8 failures)

#### RC-G1: Battle Opponent Level Higher
**Pattern**: `このカードのバトル相手のレベルが[相手のレベルより高い|0以下]なら`
**Affected**: NIK/S117-051 (second row), NIK/S117-061 (first row)
**Fix**: Create `BattleOpponentLevelConditionToken`.
**Status**: Missing.

#### RC-G2: Reverse Character Optional
**Pattern**: `あなたはそのキャラを【リバース】してよい。`
**Affected**: NIK/S117-061 (first row)
**Fix**: Create `ReverseCharacterOptionalToken`.
**Status**: Missing.

---

### RC-H: Search/Deck Manipulation (Low Impact - ~10 failures)

#### RC-H1: Search Level X or Lower Trait
**Pattern**: `レベルX以下の《NIKKE》のキャラを1枚選び`
**Affected**: NIK/S117-054, NIK/S117-059
**Fix**: Create `SearchLevelXOrLowerTraitToken`.
**Status**: Missing.

#### RC-H2: Reveal Top Card If Trait Add to Hand
**Pattern**: `山札の上から1枚を公開する。そのカードが《NIKKE》のキャラなら手札に加える。`
**Affected**: NIK/S117-069 (first row), NIK/S117-071 (first row), NIK/S117-079 (third row), NIK/S117-099 (second row), NIK/S117-100
**Fix**: Create `RevealTopCardIfTraitAddToHandToken`.
**Status**: Missing.

---

### RC-I: Damage/Clock Tokens (Low Impact - ~8 failures)

#### RC-I1: Trigger Check 2 Times
**Pattern**: `トリガーステップにトリガーチェックを2回行う。`
**Affected**: NIK/S117-062 (second row), NIK/S117-098 (second row), NIK/S117-104 (second row)
**Fix**: Create `TriggerCheckTwoTimesToken`.
**Status**: Missing.

#### RC-I2: Put Clock to WR or Stock
**Pattern**: `クロックの上から1枚を、控え室に置く。...控え室に置くかわりにストック置場に置いてよい。`
**Affected**: NIK/S117-079 (second row)
**Fix**: Create `PutClockToWrOrStockToken`.
**Status**: Missing.

---

### RC-J: ACT Effect Tokens (Low Impact - ~5 failures)

#### RC-J1: ACT Cost Put Card to WR
**Pattern**: `［(1) このカードを控え室に置く］`
**Affected**: NIK/S117-004, NIK/S117-012 (second row), NIK/S117-070 (second row), NIK/S117-103
**Fix**: Extend `ActEffectToken` cost parsing.
**Status**: Partial.

---

### RC-K: Other Missing Tokens (Scattered - ~20 failures)

| Token Name | Pattern | Affected Serials |
|------------|---------|------------------|
| `ExchangeLevelWithWrToken` | `レベル置場のカードと控え室のカードを入れ替える` | NIK/S117-047 |
| `CannotUseActUntilEndOfTurnToken` | `次の相手のターンの終わりまで、相手は舞台にいるキャラの【起】を使えない` | NIK/S117-052 |
| `DealDamageXTimesToken` | `相手に1ダメージを2回与える` | NIK/S117-065 |
| `PutOpponentClockToWrToken` | `相手のクロックの上から1枚を、控え室に置いてよい` | NIK/S117-053 |
| `MoveToOpenPositionToken` | `前列のキャラのいない枠に動かしてよい` | NIK/S117-106 |
| `RestIfCxExistsToken` | `それらのカードにCXがあるなら、このカードを【レスト】する` | NIK/S117-106 |
| `DrawPhaseStartConditionToken` | `相手のドローフェイズの始めに` | NIK/S117-110 |
| `RestStandCharacterToken` | `【スタンド】しているキャラを1枚選び、【レスト】し` | NIK/S117-110 |
| `DealXDamageToken` | `相手にXダメージを与える。Xはそのキャラのソウルに等しい。` | NIK/S117-110 |
| `SearchLevel0OrLowerToken` | `レベル0以下のキャラを1枚まで選び` | NIK/S117-109 |
| `PutTopXCardsToWrToken` | `山札の上からX枚を、控え室に置いてよい。Xは...` | NIK/S117-103 |
| `NoColorCardsInLevelConditionToken` | `レベル置場に色カードがないなら` | NIK/S117-079 |
| `OpponentCannotUseActOnStageToken` | `相手は舞台にいるキャラの【起】を使えない` | NIK/S117-052 |

---

### RC-L: Sub-Ability Granting (High Impact — ~15 failures)
**Pattern**: `このカードは次の能力を得る。『【自】...』` or `次の2つの能力を得る。『...』『...』`
- Token exists (`GivesAbilityToken` or similar) but output format doesn't match CSV
- Nested abilities inside `『』` are not being translated correctly
- **Affected**: NIK/S117-060, 064, 065, 087, 102
- **Status**: Missing/partial — tokens exist but output format diverges

### RC-M: Give Ability to All Characters (Low Impact — ~3 failures)
**Pattern**: `あなたのキャラすべてに、そのターン中、次の能力を与える。`
- **Affected**: NIK/S117-045
- **Status**: Missing

### RC-N: "All Players Perform" Pattern (Low Impact — ~1 failure)
**Pattern**: `すべてのプレイヤーは次の行動を行う。`
- **Affected**: NIK/S117-049
- **Status**: Missing

### RC-O: Level-Dependent Multi-Clause Effect (Low Impact — ~1 failure)
**Pattern**: `あなたのレベルが1なら...あなたのレベルが2なら...あなたのレベルが3なら...`
- Multi-branch effect within a single ability clause
- **Affected**: NIK/S117-058
- **Status**: Missing

### RC-P: "When Placed from Hand OR Attacks" Combined Condition (Low Impact — ~2 failures)
**Pattern**: `手札から舞台に置かれた時かアタックした時`
- Combined trigger condition (`時か時`)
- **Affected**: NIK/S117-075
- **Status**: Missing — existing tokens handle each trigger separately

### RC-Q: "When Playing from Hand" Condition (CONT) (Low Impact — ~1 failure)
**Pattern**: `手札のこのカードをプレイするにあたり`
- Special CONT condition for hand-play cost reduction
- **Affected**: NIK/S117-067
- **Status**: Missing

### RC-R: "Damage Not Canceled" Condition (Low Impact — ~1 failure)
**Pattern**: `ダメージがキャンセルされなかった時`
- **Affected**: NIK/S117-053
- **Status**: Missing

### RC-S: Cost: Put Colored Character at Bottom of Clock (Low Impact — ~1 failure)
**Pattern**: `あなたの控え室の[色]のキャラを1枚クロック置場の下に置く`
- Cost with color-specific character placement
- **Affected**: NIK/S117-085
- **Status**: Missing

### RC-T: Deck Count Condition (Low Impact — ~2 failures)
**Pattern**: `あなたの山札が[N]枚以下なら`
- **Affected**: NIK/S117-086
- **Status**: Missing

### RC-U: "CX Among Revealed Cards" Conditional (Low Impact — ~2 failures)
**Pattern**: `それらのカードにCXがあるなら`
- Condition checking revealed/flipped cards for CX
- **Affected**: NIK/S117-088
- **Status**: Missing

### RC-V: Complex CONT with Multiple AND/OR Conditions (Low Impact — ~2 failures)
**Pattern**: `他のあなたの《NIKKE》のキャラが3枚以上で、他のあなたの「...」がいないなら`
- Multiple conditions combined with `で` (AND logic)
- Output format: "and" vs "if" vs comma — mismatches expected CSV
- **Affected**: NIK/S117-092
- **Status**: Partial — tokens partially parse but conjunction style wrong

### RC-W: "Level Equal to or Lower Than" Search Condition (Low Impact — ~2 failures)
**Pattern**: `自分のレベル以下のレベルの`
- Two-level comparison (card level ≤ your level)
- **Affected**: NIK/S117-094
- **Status**: Missing

### RC-X: Move Opponent Character to Open Position (Low Impact — ~2 failures)
**Pattern**: `相手の舞台のキャラのいない他の枠に動かす`
- **Affected**: NIK/S117-095
- **Status**: Missing — `MoveToOpenPositionToken` exists but may not match opponent-targeting variant

### RC-Y: "When Put to WR from Stage" (Not Just Placed) (Low Impact — ~5 failures)
**Pattern**: `このカードが舞台から控え室に置かれた時`
- vs. existing `手札から舞台に置かれた時` (placed from hand)
- Different trigger condition that may not be handled
- **Affected**: NIK/S117-085, 096
- **Status**: Partial — condition token may exist but effect after it fails

### RC-Z: "No Other Trait Character" Negative Condition (Low Impact — ~1 failure)
**Pattern**: `他のあなたの《NIKKE》のキャラがいないなら`
- Negative existence check for trait characters
- **Affected**: NIK/S117-097
- **Status**: Missing

### RC-AA: "When You Use Backup" Effect (Low Impact — ~3 failures)
**Pattern**: `あなたがこのカードの『助太刀』を使った時`
- Backup-specific trigger that grants sub-abilities
- **Affected**: NIK/S117-101
- **Status**: Missing

### RC-AB: "Cannot Play Events/Backup" Grant (Low Impact — ~4 failures)
**Pattern**: `相手はイベントを手札からプレイできない` / `相手は『助太刀』を手札からプレイできない`
- `OpponentCannotPlayEventsToken` exists (matches exact text) but:
  - Output format doesn't match CSV (word order)
  - When combined with battle condition, parsing fails
- **Affected**: NIK/S117-064, 102
- **Status**: Partial — token exists but output format/CONT embedding fails

### RC-AC: effects_550.csv (ANM/W138) Not Covered at All (Low Impact — ~2 failures)
**Pattern**: Completely separate expansion file `effects_550.csv` (ANM prefix)
- **Affected**: ANM/W138-T11, ANM/W138-T12
- **Root Causes**:
  - T11: Output format mismatch ("to the bottom" vs "at the bottom") — CSV value issue
  - T12: `AssistPowerBoostToken` regression — regex matches fixed power +500 as if it were variable X with level multiplier
- **Status**: Missing — entire expansion not addressed in plan

### RC-AD: Multi-Clause Ability Parsing (⚠️ Biggest Root Cause — ~40+ failures)

**Pattern**: Multiple effect clauses separated by `、` or `。` or combined with `し` / `て`
- This is NOT a missing token issue — individual tokens exist for sub-patterns
- The parsing pipeline stops at the first matched ability clause and discards the rest
- Many AUTO effects have the structure: `[condition]、[action1]、[action2]、[action3]`
- **Status**: Architectural — requires changes to how abilities are parsed sequentially

#### Core Problem

Japanese abilities use multiple sentence/clause structures that the current parser cannot handle:

1. **Sentence boundaries (`。`)**: Parser does not split on `。` (unlike `EventEffectToken`/`BrainstormEffectToken` which do)
2. **Continuation particles (`し、` / `て、`)**: Only `ContEffectToken` handles `し、` via `ConditionalLeadInPrefixes`
3. **Cost parsing via `GetMatch`**: Requires exact match at start; throws on multi-clause costs
4. **Lead-in prefix exhaustion**: Parser skips known prefixes once, then breaks
5. **`Match` strictness**: Only returns index-0 matches. Non-zero index matches log warning with skipped text.
6. **Nested `『』` abilities**: `TryTranslateNested` doesn't handle multi-clause nested text

#### Current Parsing Architecture Gap

| Token | Sentence Split (`。`) | `し` Handling | Prefix Skipping | Cost Parsing |
|-------|----------------------|---------------|-----------------|--------------|
| `EventEffectToken` | ✅ Yes | ❌ No | ❌ No (breaks) | N/A |
| `BrainstormEffectToken` | ✅ Yes | ❌ No | ❌ No (char skip) | N/A |
| `AutoEffectToken` | ❌ No | ❌ No | ✅ Limited | `GetMatch` |
| `ActEffectToken` | ❌ No | ❌ No | ❌ No (breaks) | `GetMatch` |
| `ContEffectToken` | ❌ No | ✅ Yes | ❌ No (throws) | N/A |

---

#### Failure Pattern Groups (8 Groups)

##### Group AD-1: Sentence Boundary (`。`) Not Split — ~12 failures

**Pattern**: Multiple sentences separated by `。` within a single effect. Parser processes as one continuous stream.

**Examples**:
- NIK/S117-046: `手札に戻す。...控え室に置いてよい。そうしたら、...手札に戻す` (3 sentences, cascading optional)
- NIK/S117-014: `手札に加える。手札に加えたなら、...` (2 sentences, second has conditional)
- NIK/S117-065: `相手に1ダメージを与える。この能力を使うには...` (2 sentences)

**Affected**: `AutoEffectToken`, `ActEffectToken`, `ContEffectToken`

**Fix**: Add `。` sentence splitting (protecting `『』` content) to all three effect tokens, matching `EventEffectToken` pattern.

##### Group AD-2: Continuation Particle `し、` Not Handled — ~8 failures

**Pattern**: Abilities connected by `し、` (continuative form "and also"). Only `ContEffectToken` handles this.

**Examples**:
- NIK/S117-076: `パワーを＋2000し、ソウルを＋1。` (power boost + soul boost)
- NIK/S117-011: `パワーを＋1000し、次の能力を与える。『【自】...』` (power boost + give ability)
- NIK/S117-052: `見て...選び...置き...し、使えない` (5+ actions connected by `し、`)

**Affected**: `AutoEffectToken`, `ActEffectToken`

**Fix**: Add `し、`, `し`, `て、`, `て` to `AbilityLeadInPrefixes` in `AutoEffectToken`. Add prefix array to `ActEffectToken`.

##### Group AD-3: Cost Token Matching via `GetMatch` — ~7 failures

**Pattern**: Cost text from `［...］` passed to `EffectListRegistry.GetMatch()`. `GetMatch` requires exact match at start and throws if no token matches. Cost text may contain multiple clauses.

**Examples**:
- NIK/S117-044: `［手札の《NIKKE》のキャラを1枚控え室に置く］` — cost is full ability clause
- NIK/S117-068: `［あなたの《NIKKE》のキャラを2枚【レスト】する］` — cost with count
- NIK/S117-108: `［他のあなたの【スタンド】している《NIKKE》のキャラを1枚【レスト】する］` — cost with condition

**Affected Files**: `AutoEffectToken.cs:92-94`, `ActEffectToken.cs:25-27`

**Fix**: Replace `GetMatch` with `Match` loop with strict index-0 enforcement.

##### Group AD-4: Lead-In Prefix Exhaustion — ~6 failures

**Pattern**: After matching ability, remaining text starts with prefix not in `AbilityLeadInPrefixes`. Parser skips once, then breaks.

**Current prefixes**: `["あなたは", "あなたの", "自分の", "そうしたら、", "そうしたら", "その後、", "その後", "次の", "そして、", "そして"]`

**Missing prefixes**: `し、`, `し`, `て、`, `て`, `そうでなければ、`, `そうしなければ、`, `このカードは`, `このカードが`, `相手の`, `他の`

**Examples**:
- NIK/S117-052: `相手の【レスト】しているキャラ...` — starts with `相手の`
- NIK/S117-092: `他のあなたの《NIKKE》のキャラが3枚以上で...` — starts with `他の`

**Fix**: Expand `AbilityLeadInPrefixes` + enable recursive prefix skipping (skip → retry match → skip again).

##### Group AD-5: `Match` Strictness — Index-0 Only — ~5 failures

**Pattern**: `TokenRegistry.Match` only returns index-0 matches. Tokens matching at index > 0 are not returned — instead a warning is logged with skipped text. General token at index 5 is ignored in favor of no match, forcing prefix skipping or regex fix.

**Examples**:
- NIK/S117-078: `PowerBoostPerOtherNikkeToken` exists but `Match` returns null (matched at non-zero index)
- NIK/S117-099: Same — token exists but registry doesn't find it at index 0

**Fix**: `Match` enforces index-0 only. Callers must skip lead-in prefixes before calling `Match`, or fix token regex to anchor at `^`.

##### Group AD-6: Nested Ability in `『』` Not Fully Translated — ~4 failures

**Pattern**: Abilities granting sub-abilities use `『【自】...』` format. `TryTranslateNested()` has limitations with multi-clause nested text.

**Examples**:
- NIK/S117-060: `次の能力を得る。『【自】...手札に戻す。...控え室に置く』` — nested multi-clause
- NIK/S117-064: `次の能力を得る。『【永】相手はイベントを手札からプレイできない』` — nested CONT
- NIK/S117-102: Similar nested ability grant

**Fix**: Apply sentence splitting and prefix handling within `TryTranslateNested`.

##### Group AD-7: Condition Chain with `で`/`て`/`く` Connectors — ~4 failures

**Pattern**: Multiple conditions chained with particles before final `なら`:
```
CX置場に「...」があり、前列にこのカードがいて、...が2枚以上で、...がいないなら
```

**Examples**:
- NIK/S117-002: `あり、いて、...で、...なら` (4+ chained conditions)
- NIK/S117-030: Similar chain with CX + position + count conditions

**Fix**: Condition tokens need `で`/`て` variant patterns, OR condition parser splits on `、` and matches each segment independently. (Overlaps with RC-AG.)

##### Group AD-8: Cascading Optional Actions (`そうしたら` / "If You Do") — ~3 failures

**Pattern**: Optional action followed by cascading effect:
```
手札に戻す。...控え室に置いてよい。そうしたら、...手札に戻す
```

**Examples**:
- NIK/S117-046: `手札に戻す。...置いてよい。そうしたら、...戻す`
- NIK/S117-069: Similar cascade

**Fix**: After skipping `そうしたら`, re-enter full condition + ability parsing loop, not just ability matching.

---

#### Implementation Plan (7 Phases)

##### Phase AD-0: Entity Restructuring (Prerequisite)

**Goal**: Add `Prefix` to `CardEffectAbility` and `Conjunction` to `CardEffectCondition` so lead-in prefixes and condition connectors are captured as structured data.

**Files Modified**: `CardEffectAbility.cs`, `CardEffectCondition.cs`

**New Types**:
```csharp
public enum AbilityPrefix { And, Continuation, IfYouDo, Otherwise, AfterThat, Subject }
public enum ConditionConjunction { And, Or, Continuation }
```

**Impact**: Default values produce identical output to current behavior. New parsing logic sets these properties for correct English conjunctions.

##### Phase AD-0.5: Token Registry Unification + Debug Logging (Prerequisite)

**Goal**: Merge `GetMatch`, `TryMatchAtStart`, `TryFindFirstMatch` into single `TokenMatch Match(ReadOnlyMemory<char> input)`. Add `TokenMatch` record (`Input`, `Index`, `Length`, `Token`). Add `List<string> TokenLog` to `CardEffect`. **Strict matching: only index-0 matches returned.** Non-zero index matches log warning via Serilog.Log with skipped text details.

**Files Modified**: `TokenRegistry.cs`, `CardTextToken.cs`, `CardEffect.cs`, all effect tokens

**New API**:
```csharp
public record TokenMatch(ReadOnlyMemory<char> Input, int Index, int Length, string Token);
TokenMatchResult<E>? Match(ReadOnlyMemory<char> input); // Only returns index-0 matches
```

**Strictness behavior**:
- Index-0 match → returned immediately (registration order preserved)
- No index-0 match but tokens matched mid-string → `Log.Warning` with skipped text, which tokens matched at which indices, returns `null`
- No match at all → `Log.Warning`, returns `null`
- Caller handles `null` by throwing or skipping known lead-in prefixes

**Impact**: Stricter matching forces all tokens to anchor at `^`. Non-zero index matches are diagnostic only, never consumed. `TokenLog` enables debugging which tokens matched in what order.

##### Phase AD-1: Extract Shared Multi-Clause Parser (Foundation)

**New File**: `Montage.Weiss.Tools/Entities/Effect/Token/MultiClauseEffectParser.cs`

**Responsibilities**:
1. Split input on `。` (protecting `『』` content)
2. For each sentence: iteratively match conditions (tracking `Conjunction`), then abilities (detecting `Prefix`)
3. Handle continuation particles (`し、`, `て、`, `し`, `て`) → `Continuation`
4. Detect lead-in prefixes via `LeadInPrefixMap` → set `AbilityPrefix` on results
5. Join results using `Prefix`/`Conjunction` values (not hardcoded punctuation)

**Public API**:
```csharp
public static class MultiClauseEffectParser
{
    public static ParsedEffect Parse(string input, ITokenRegistry registry, LeadInPrefixMap? prefixMap = null);
    public static ParsedSentence ParseSentence(string sentence, ITokenRegistry registry, LeadInPrefixMap? prefixMap = null);
    public static (AbilityPrefix Prefix, string Rest) DetectLeadInPrefix(string input, LeadInPrefixMap? map = null);
    public static ConditionConjunction DetectConditionConjunction(string input, out string stripped);
}
```

**Files Modified**: `AutoEffectToken.cs`, `ActEffectToken.cs`, `ContEffectToken.cs` — delegate to parser

**Tests**: Verify no regressions in 100+ currently passing tests.

##### Phase AD-2: Fix Cost Token Matching

**Files Modified**: `AutoEffectToken.cs:92-94`, `ActEffectToken.cs:25-27`

**Change**: Replace `GetMatch` with `Match` loop with **strict index-0 enforcement**. Costs are `CardEffectAbility` sequences — each token must match at the start of remaining text. No prefix skipping allowed.

```csharp
var matchResult = registry.EffectListRegistry.Match(t.AsMemory());
if (matchResult == null) break;
if (matchResult.Match.Index > 0)
    throw new NotImplementedException($"Cost token '{matchResult.Match.Token}' matched at index {matchResult.Match.Index} instead of 0");
```

**Example**: Cost `[(1) [REST] this card & Put top card to clock]` parses as 3 sequential index-0 matches.

**Tests**: Verify NIK/S117-044, 060, 068, 069, 108 cost parsing.

##### Phase AD-3: Lead-In Prefix Map + Recursive Skipping

**Files Modified**: `MultiClauseEffectParser.cs`, `AutoEffectToken.cs`, `ActEffectToken.cs`

**Change**: Replace string array `AbilityLeadInPrefixes` with typed `LeadInPrefixMap` mapping each prefix to its `AbilityPrefix` enum. Enable recursive prefix skipping for **abilities only** — costs use strict index-0 matching without prefix skipping.

**Key Distinction**:
- **Costs**: Strict index-0. Each token must match at start. No prefix skipping.
- **Abilities**: Prefix detection → `AbilityPrefix` → skip prefix → match → set prefix on result.

**Prefix Map** (key patterns → enum values):
- `し、`/`し`/`て、`/`て` → `Continuation`
- `そうしたら、`/`そうしたら` → `IfYouDo`
- `そうでないなら`/`そうでなければ`/`そうしなければ` → `Otherwise`
- `その後、`/`その後` → `AfterThat`
- `あなたは`/`自分の`/`このカードは`/`相手の`/`他の` → `Subject`
- `そして、`/`そして` → `Continuation`

**New Prefixes**: `し、`, `し`, `て、`, `て`, `そうでなければ、`, `そうでなければ`, `そうしなければ、`, `そうしなければ`, `このカードは`, `このカードが`, `相手の`, `他の`

**Recursive Skipping**: After skipping prefix, fall through to next iteration for re-match (not just `continue`).

##### Phase AD-4: Match Strictness (Handled by AD-0.5)

**File Modified**: `TokenRegistry.cs:69-88`

**Change**: Two-pass approach:
1. First pass: check for index-0 matches in registration order (return immediately on first match)
2. Second pass: find earliest non-zero index match

**Rationale**: Registration order (most specific → most general) should be respected at index 0.

##### Phase AD-5: Enhance Nested Ability Translation

**File Modified**: `GainFollowingAbilityToken.cs` — `TryTranslateNested` method

**Change**: Use `MultiClauseEffectParser.ParseSentence` for each nested sentence before falling back to existing split/stripping logic.

##### Phase AD-6: Integration + Refactoring

**Files Modified**: `AutoEffectToken.cs` (lines 97-172), `ActEffectToken.cs` (lines 29-56), `ContEffectToken.cs` (lines 70-127)

**Goal**: Replace duplicated parsing logic with `MultiClauseEffectParser` calls. Differences (like `し` handling) via `additionalLeadInPrefixes` parameter.

---

#### Expected Impact per Phase

| Phase | Est. Tests Fixed | Risk | Dependencies |
|-------|-----------------|------|-------------|
| AD-0: Entity Restructuring | +0 (infrastructure) | Low | None |
| AD-0.5: Registry Unification | +0 (infrastructure) | Medium (API change across all tokens) | AD-0 |
| AD-1: Multi-Clause Parser | +15-20 | Medium | AD-0, AD-0.5 |
| AD-2: Cost Token Matching | +7 | Low | AD-0, AD-0.5 |
| AD-3: Prefix Expansion | +8-10 | Low | AD-0, AD-0.5, AD-1 |
| AD-4: Match Priority Fix | +5 | Low (handled by AD-0.5) | AD-0, AD-0.5 |
| AD-5: Nested Ability Fix | +4 | Low | AD-0, AD-0.5, AD-1 |
| AD-6: Integration | +5-10 | High | AD-0 through AD-5 |

**Total Estimated**: +40-55 tests fixed (from ~100 to ~140-155 passing)

#### Key Constraints
- **Do NOT create new tokens for RC-AD** — tokens already exist, problem is parsing pipeline
- **CSV updates wait** until RC-AD is stable (output format may change)
- **Strict matching distinction**: Costs require index-0 matches (no prefix skipping). Abilities use prefix detection + skipping. `Match` enforces this by only returning index-0 matches — non-zero index matches log skipped text via Serilog and return null.
- **Phase ordering**: AD-1 before AD-6; AD-2/AD-3/AD-4 can be parallel with AD-1
- **Quick win**: Adding `し、` to prefixes (AD-3) fixes ~8 tests independently

### RC-AE: CSV Expected Value / Output Format Mismatch (Medium Impact — ~15 failures)
**Pattern**: Token correctly parses Japanese text but produces different English output than CSV expects
- **Not a translation bug per se** — the token works but uses different wording
- **Examples**:
  - NIK/S117-086: "If there is a CX" (CSV) vs "If a CX is" (actual)
  - NIK/S117-092: "and you do not have" (CSV) vs "if you do not have" (actual)
  - NIK/S117-095: "search your deck" (CSV) vs "choose up to 1 ... from among them" (actual)
  - NIK/S117-096: Missing "choose up to 1 level 1 or higher card" (filter lost)
  - NIK/S117-110: Untranslated Japanese `レベル3以下の` and wrong `DealXDamageToken` output
- **Status**: Needs CSV update OR token translation adjustment

### RC-AF: Top-Level Parsing Gap — Effects Without 能力 Prefix (Low Impact — ~1 failure)
**Pattern**: Effect text with no `【永】/【自】/【起】/【Act】` prefix
- **Affected**: NIK/S117-080 — `あなたは自分の山札を見てCXを...` (standalone search effect)
- **Status**: Missing — parser likely fails to match anything when no effect type prefix

### RC-AG: Complex Chain Conditions with で/て/く Connectors (Medium Impact — ~8 failures)
**Pattern**: Multiple conditions chained with particles `で`, `て`, `く`, ending with `なら`
- **Examples**:
  - `CX置場に「...」があり、前列にこのカードがいて、...が2枚以上で、...がいないか【リバース】しているなら`
  - Each segment is a separate condition, but the parser tries to match as a single pattern
  - The OR variant `いないか【リバース】している` (is absent OR is reversed) is particularly problematic
- **Affected**: NIK/S117-002, 003, 004, 005, 030, 034, 035, 036, 037
- **Status**: Missing — no condition-chain token exists

### RC-AH: AssistPowerBoostToken Regression — Fixed Power Treated as Variable X (Regressed — ~2 failures)
**Pattern**: `パワーを＋500` (fixed number) matched by AssistPowerBoostToken regex that expects `パワーを＋(?:X|\d+)`
- **Root Cause**: RC-A1 fix made regex accept `\d+` in addition to `X`, but translation code always outputs the "X is equal to level" format even when no level multiplier matched
- **Evidence**: ANM/W138-T12 output shows `get +X power. X is equal to that character's level x0` instead of `get +500 power`
- **Affected**: ANM/W138-T12, possibly NIK/S117-072 (fixed Assist +1500)
- **Status**: Token has bug — needs fix to distinguish fixed numbers from variable X

---

## Token Implementation Guidelines

All new or modified tokens **MUST** follow `Montage.Weiss.Tools\Entities\Effect\Token\README.md`. Key requirements:

### Mandatory Rules
1. **All regex patterns MUST start with `^`** - enforced by `Registry_RegexMustStartWithAnchor` test
2. **Ability tokens MUST end with `(?:\.|,|、|。)?`** - enforced by `Registry_AbilitiesMustCaptureEndingPunctuations` test
3. **Condition tokens MUST be atomic** - no conjunctions or multiple conditions (enforced by `Registry_ConditionsMustBeAtomic` test)

### Documentation Requirements
Every new/modified token class **MUST** include XML documentation:
```csharp
/// <summary>
/// Brief description of what this token matches.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>Japanese text example here</c></para>
/// <para><b>Regex:</b> ^your-regex-pattern-here</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: What this group captures</description></item>
/// </list>
/// <para><b>Output:</b> <c>English translation example</c></para>
/// <para><b>Type:</b> <c>ConditionType.When</c></para> <!-- For condition tokens only -->
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Variation 1 description
/// - Variation 2 description</para>
/// </remarks>
```

### Regex Best Practices
- Use `\s*` for optional whitespace variations
- Handle both full-width and half-width characters (e.g., `X` and `X`)
- Use `(?:pattern1|pattern2)` for alternative patterns
- Use named capture groups `(?<name>...)` for clarity
- Use lazy matching `.+?` instead of greedy `.+` when appropriate

### Token Categories
Choose the correct category based on what the token parses:
- **Effect Tokens** (`CardTextToken<CardEffect>`): Parse top-level effect types (【永】, 【自】, 【起】, etc.)
- **Ability Tokens** (`CardTextToken<List<CardEffectAbility>>`): Parse individual ability clauses within effects
- **Condition Tokens** (`CardTextToken<List<CardEffectCondition>>`): Parse conditional clauses (when, if, during)
- **Reminder Text Tokens** (`CardTextToken<string>`): Parse parenthetical reminder text

### Registration
Register tokens in `WeissSchwarzCardTranslatorService`:
- Effect tokens → `_effectRegistry.Register()`
- Ability tokens → `_effectListRegistry.Register()`
- Condition tokens → `_conditionListRegistry.Register()`
- Reminder text tokens → `_reminderTextRegistry.Register()`

---

## Implementation Priority

### Phase 1: High Impact (COMPLETED — 2/18 test rows fixed)
**Result**: 98 → 100 passed (+2). Most Phase 1 tokens exist but fail due to RC-AD (multi-clause parsing).

1. **RC-A1**: Fixed `AssistPowerBoostToken` regex ✅ (distinguishes fixed vs variable X)
   - ⚠️ NIK/S117-015 row 1 still fails → **Re-classified to RC-AG** (prefix `あなたのターン中、` blocks match)
   - ⚠️ NIK/S117-091 row 1 still fails → **Re-classified to RC-AE** (regex missing trait `《NIKKE》` variation)
2. **RC-A2**: `PowerBoostPerOtherNikkeToken` created ✅ but not matching
   - NIK/S117-078 row 1 → **Re-classified to RC-AD** (token exists, registry not finding it)
   - NIK/S117-099 row 1 → **Re-classified to RC-AD** (same)
3. **RC-A3**: `PowerBoostPerOpponentRestToken` created ✅ but not matching
   - NIK/S117-052 row 1 → **Re-classified to RC-AD** (token exists, registry not finding it)
4. **RC-A4**: `SoulBoostOneToken` created ✅ but not matching after `し、` strip
   - NIK/S117-076 row 1 → **Re-classified to RC-AD** (multi-clause: `し、ソウルを＋1。`)
5. **RC-B1**: `CostPutTraitCharacterFromHandToWaitingRoomToken` exists ✅ but not matching in cost context
   - NIK/S117-044 row 2 → **Re-classified to RC-AD** (cost parsing via GetMatch, not Match)
   - NIK/S117-060 row 3 → **Re-classified to RC-AD** (same)
   - NIK/S117-069 row 2 → **Re-classified to RC-AD** (same)
6. **RC-B2**: `CostRestStandNikkeCharacterToken` created ✅ but not matching in cost context
   - NIK/S117-108 row 2 → **Re-classified to RC-AD** (same as RC-B1)
7. **RC-B3**: `CostRestTwoNikkeCharactersToken` created ✅ but not matching in cost context
   - NIK/S117-068 row 2 → **Re-classified to RC-AD** (same as RC-B1)
8. **RC-B4**: `CostPutToStockAndSwapBottomToken` created ✅ but not matching in cost context
   - NIK/S117-063 row 2 → **Re-classified to RC-AD** (same as RC-B1)

**Phase 1 Re-classification Summary**:
| Original RC | Affected Rows | Re-classified To | Reason |
|-------------|---------------|------------------|--------|
| RC-A1 | NIK/S117-015 row 1 | RC-AG | Condition prefix `あなたのターン中、` before ability |
| RC-A1 | NIK/S117-091 row 1 | RC-AE | Regex missing `《NIKKE》` trait variation |
| RC-A2 | NIK/S117-078 row 1 | RC-AD | Token exists but registry not matching |
| RC-A2 | NIK/S117-099 row 1 | RC-AD | Token exists but registry not matching |
| RC-A3 | NIK/S117-052 row 1 | RC-AD | Token exists but registry not matching |
| RC-A4 | NIK/S117-076 row 1 | RC-AD | Multi-clause: `し、` connector + ability |
| RC-B1 | NIK/S117-044 row 2 | RC-AD | Cost token not matched in GetMatch context |
| RC-B1 | NIK/S117-060 row 3 | RC-AD | Cost token not matched in GetMatch context |
| RC-B1 | NIK/S117-069 row 2 | RC-AD | Cost token not matched in GetMatch context |
| RC-B2 | NIK/S117-108 row 2 | RC-AD | Cost token not matched in GetMatch context |
| RC-B3 | NIK/S117-068 row 2 | RC-AD | Cost token not matched in GetMatch context |
| RC-B4 | NIK/S117-063 row 2 | RC-AD | Cost token not matched in GetMatch context |
| RC-AH | ANM/W138-T12 | ✅ FIXED | AssistPowerBoostToken now handles fixed numbers |
| RC-AC | ANM/W138-T11 | RC-AE | Output format: "to the bottom" vs "at the bottom" |

### Phase 2: Medium Impact (Fix 30+ tests)
9. **RC-C1**: Create `GainEncoreAbilityToken`
10. **RC-C2**: Create `GiveEncoreToAllOpponentCharactersToken`
11. **RC-D1**: Fix `CxNamedPlacedConditionToken` quote styles
12. **RC-D2**: Create `CardExistsInMemoryConditionToken`
13. **RC-E1**: Add treasure/choice/standby icons to reminder text
14. **RC-F1**: Create `PlacedFromHandPowerBoostToken`
15. **RC-F2**: Extend `LookAtTopCardsToken` for follow-up actions

### Phase 3: Low Impact (Fix 40+ tests)
16. **RC-G1**: Create `BattleOpponentLevelConditionToken`
17. **RC-G2**: Create `ReverseCharacterOptionalToken`
18. **RC-H1**: Create `SearchLevelXOrLowerTraitToken`
19. **RC-H2**: Create `RevealTopCardIfTraitAddToHandToken`
20. **RC-I1**: Create `TriggerCheckTwoTimesToken`
21. **RC-I2**: Create `PutClockToWrOrStockToken`
22. **RC-J1**: Extend `ActEffectToken` cost parsing
23. **RC-K**: Create remaining 12 tokens

### Phase 4: New RC Categories — Individual Tokens (Fix 20+ tests)
24. **RC-L**: Sub-ability granting with nested `『』` ability translation
25. **RC-M**: "Give ability to all characters" token
26. **RC-P**: OR combined trigger condition token (`時か時`)
27. **RC-R**: "Damage not canceled" condition token
28. **RC-S**: Color-specific clock cost token
29. **RC-T**: Deck count condition token
30. **RC-U**: "CX among revealed" condition token
31. **RC-W**: "Level equal/lower than your level" token
32. **RC-Z**: "No other trait" negative condition token
33. **RC-AA**: "When you use backup" trigger token
34. **RC-AG**: Complex chain conditions with で/て/く connectors

### Phase 5: Architecture — Multi-Clause Parsing (Fix 40+ tests)
**Detailed plan**: See RC-AD section above — 8 sub-phases (AD-0 through AD-0.5, AD-1 through AD-6). Full details in `.plans/RC-AD_Comprehensive_Fix_Plan.md`.

35. **AD-0**: Entity restructuring — add `AbilityPrefix` to `CardEffectAbility`, `ConditionConjunction` to `CardEffectCondition`
36. **AD-0.5**: Token registry unification — merge 3 match methods into `TokenMatch Match()`, only returns index-0 matches, logs skipped text via Serilog for non-zero matches, adds `TokenLog` to `CardEffect`
37. **AD-1**: Extract `MultiClauseEffectParser` — shared sentence splitting + iterative parsing + prefix/conjunction tracking (+15-20 tests)
38. **AD-2**: Fix cost token matching — `Match` loop with strict index-0 (costs = sequential abilities, no prefix skipping) (+7 tests)
39. **AD-3**: Lead-In Prefix Map — typed prefix→enum mapping + recursive skipping (+8-10 tests)
40. **AD-4**: Two-pass `Match` — index-0 first, then earliest (+5 tests)
41. **AD-5**: Enhance nested `『』` ability translation with `MultiClauseEffectParser` (+4 tests)
42. **AD-6**: Integrate — replace duplicated logic in all effect tokens (+5-10 tests)

### Phase 6: Cross-Expansion & Regression Fixes (Fix 10+ tests)
37. **RC-AC**: Add effects_550.csv test data and fix ANM/W138 failures
38. **RC-AH**: Fix AssistPowerBoostToken regression (fixed vs variable X)
39. **RC-V**: Fix CONT conjunction style ("and" vs "if")
40. **RC-AE**: Align token output formats with CSV expected values

### Phase 7: CSV Fixes (Fix 15+ tests)
41. Fix quote styles in CSV (Japanese vs English quotes)
42. Fix trigger icon formats (`.gif` → `[ICON]`)
43. Fix capitalization inconsistencies
44. Update CSV expected values to match corrected token output

---

## Testing Strategy

### Full Test Suite

```powershell
# After each phase, run full test suite:
dotnet build
dotnet test --filter "FullyQualifiedName~Translate_CSV_CrossCheckAll" --no-build

# Or with build:
dotnet test --filter "FullyQualifiedName~Translate_CSV_CrossCheckAll"
```

### Testing Individual Serials

To test for a specific card serial (e.g., `NIK/S117-008`), use substring matching with the `~` operator on `TestCategory`:

```powershell
# Test single serial
dotnet test --filter "TestCategory~NIK/S117-008"

# Test multiple serials (OR logic)
dotnet test --filter "TestCategory~NIK/S117-008|TestCategory~NIK/S117-006"

# Test all NIK/S117-0XX serials
dotnet test --filter "TestCategory~NIK/S117-0"
```

This runs all tests whose `TestCategory` tag contains the specified serial. The test name format is `CSV-Cross-Check#<serial>#<hash>`.

**Note:** Do NOT use `FullyQualifiedName~<serial>` — this does not work because the serial is in the `TestCategory` attribute, not the test name.

### Progress Tracking

```
# Phase 1 actual:    98 → 100 pass (+2, not +22 as expected)
# Phase 1.5 actual:  100 → 119 pass (+19)
# Sprint 2 actual:   119 → 122 pass (+3)
# Phase 2 target:   122 → 150+ pass
# Phase 3 target:   150 → 170+ pass (new individual tokens)
# Phase 4 target:   170 → 190+ pass (multi-clause parsing — biggest impact)
# Phase 5 target:   190 → 210+ pass (regression + cross-expansion fixes)
# Phase 6 target:   210 → 246 pass (CSV alignment)
```

---

## Success Criteria
- [ ] All 127 failing test rows pass or explicitly reclassified
- [ ] 0 regressions in previously passing tests (currently 119)
- [ ] effects_550.csv failures also resolved (ANM/W138-T11 → RC-AE output format)
- [x] AssistPowerBoostToken regression fixed (ANM/W138-T12) — Phase 1
- [x] NIK/S117-006 fixed (nested ability grant after `そうしたら、`) — Phase 1.5
- [x] NIK/S117-008 fixed (2 rows: trait character to stock + CX with trigger icon) — Phase 1.5
- [ ] `dotnet build` passes with 0 errors
- [ ] Pre-commit hooks pass

## Test Results Summary
- **Before implementation**: 95 passed / 151 failed / 246 total
- **After token creation (plan baseline)**: 95 passed / 151 failed / 246 total
- **Pre-Phase 1 (revalidation)**: 98 passed / 148 failed / 246 total
- **After Phase 1**: **100 passed / 146 failed / 246 total** (+2 from 98)
- **After Phase 1.5 (early 2026-05-18)**: **119 passed / 127 failed / 246 total** (+19 from 100)
- **After Sprint 2 (late 2026-05-18)**: **122 passed / 124 failed / 246 total** (+3 from 119)
- **NotImplementedException count**: 29 → **24** (-5)
- **Tokens created**: 29 new token files (24 Phase 1 + 2 Phase 1.5 + 3 Sprint 2)
- **Tokens registered**: All 29 registered in WeissSchwarzCardTranslatorService
- **Tests fixed by Phase 1**: 2 (ANM/W138-T12 + 1 other from AssistPowerBoostToken fix)
- **Tests fixed by Phase 1.5**: 19 (NIK/S117-006, NIK/S117-008 x2, ANM/W138-T11, and others from token fixes)
- **Tests fixed by Sprint 2**: 5 (NIK/S117-005 x2, NIK/S117-011 x1, NIK/S117-015 x1, NIK/S117-017 x3 = net +5)
- **Phase 1 re-classified**: 16 test rows moved to RC-AD (12), RC-AE (2), RC-AG (1), already-fixed (1)
- **Remaining work**: Multi-clause ability parsing (now includes 12 re-classified Phase 1 rows), CSV value mismatches, and new RC categories below

## Completed

Note: ⚠️ Many tokens listed below as "CREATED" are still NOT passing tests. See "Token Effectiveness Audit" below.

- [x] **RC-A1**: Fixed `AssistPowerBoostToken` regex (Phase 1, item 1)
  - **✅ FIXED**: Now properly distinguishes fixed numbers (e.g., +500) from variable X with level multiplier
  - **Impact**: Fixed ANM/W138-T12 regression; NIK/S117-015/091 still fail due to other root causes (re-classified to RC-AG/RC-AE)
  - **Status**: ✅ COMPLETE (token fix done; remaining failures are different root causes)
- [x] **RC-A2**: Created `PowerBoostPerOtherNikkeToken` (Phase 1, item 2)
  - **Status**: ❌ CREATED but NIK/S117-078, 099 still failing → **Re-classified to RC-AD** (token exists but registry not matching in CONT context)
- [x] **RC-A3**: Created `PowerBoostPerOpponentRestToken` (Phase 1, item 3)
  - **Status**: ❌ CREATED but NIK/S117-052 still failing → **Re-classified to RC-AD** (token exists but registry not matching)
- [x] **RC-A4**: Created `SoulBoostOneToken` (Phase 1, item 4)
  - **Status**: ❌ CREATED but NIK/S117-076 still failing → **Re-classified to RC-AD** (multi-clause: `し、` connector + ability after condition)
- [x] **RC-B2**: Created `CostRestStandNikkeCharacterToken` (Phase 1, item 6)
  - **Status**: ❌ CREATED but NIK/S117-108 still failing → **Re-classified to RC-AD** (cost token not matched in GetMatch context)
- [x] **RC-B3**: Created `CostRestTwoNikkeCharactersToken` (Phase 1, item 7)
  - **Status**: ❌ CREATED but NIK/S117-068 still failing → **Re-classified to RC-AD** (cost token not matched in GetMatch context)
- [x] **RC-B4**: Created `CostPutToStockAndSwapBottomToken` (Phase 1, item 8)
  - **Status**: ❌ CREATED but NIK/S117-063 still failing → **Re-classified to RC-AD** (cost token not matched in GetMatch context)
- [x] **RC-C1**: Created `GainEncoreAbilityToken` (Phase 1, item 9)
  - **Status**: ❌ CREATED but NIK/S117-066, 093 still failing (nested `『』` ability not translated)
- [x] **RC-D2**: Created `CardExistsInMemoryConditionToken` (Phase 2, item 12)
  - **Status**: ❌ CREATED but NIK/S117-065 still failing (multi-clause CXCOMBO chain)
- [x] **RC-F1**: Created `PlacedFromHandPowerBoostToken` (Phase 2, item 14)
  - **Status**: ❌ CREATED but NIK/S117-063, 071 still failing (multi-clause issue)
- [x] **RC-G1**: Created `BattleOpponentLevelConditionToken` (Phase 3, item 16)
  - **Status**: ❌ CREATED but NIK/S117-051, 061 still failing (multi-clause after condition)
- [x] **RC-G2**: Created `PutCharacterToBottomOfOpponentDeckToken` (Phase 3, item 17 variant)
  - **Status**: ❌ CREATED but not matching; ANM/W138-T11 has different output wording
- [x] **RC-H1**: Created `SearchLevelXOrLowerTraitToken` (Phase 3, item 18)
  - **Status**: ❌ CREATED but NIK/S117-054, 059 still failing (Brainstorm/CXCOMBO multi-clause context)
- [x] **RC-H2**: Created `RevealTopCardIfTraitAddToHandToken` (Phase 3, item 19)
  - **Status**: ❌ CREATED but NIK/S117-069, 071, 079, 099, 100 still failing
- [x] **RC-I1**: Created `TriggerCheckTwoTimesToken` (Phase 3, item 20)
  - **Status**: ❌ CREATED but NIK/S117-062, 098, 104 still failing (multi-clause CXCOMBO context)
- [x] **RC-I2**: Created `PutClockToWrOrStockToken` (Phase 3, item 21)
  - **Status**: ❌ CREATED but NIK/S117-079 still failing (multi-clause after it fails)
- [x] **RC-K**: Created various tokens (ExchangeLevelWithWr, DealDamageXTimes, etc.)
  - **Status**: ❌ Nearly ALL still failing — tokens match but subsequent clauses are discarded

### Key Finding: Token Effectiveness Audit (Updated after Phase 1)

Despite all 24 tokens being "created and registered", the actual test pass rate only improved by **5 tests** total (95→100). Phase 1 specifically added +2 (98→100). The vast majority of "completed" tokens are NOT fixing tests because:

1. **Cost tokens use GetMatch, not Match**: AutoEffectToken/ActEffectToken extract cost text and call `GetMatch()` which requires exact match at start. Cost tokens exist but `GetMatch` throws "No token found" → **RC-AD root cause** (12 test rows re-classified)
2. **CONT ability tokens not found by Match**: PowerBoostPerOtherNikkeToken, PowerBoostPerOpponentRestToken exist but `Match` returns null (non-zero index) → **RC-AD root cause** (3 test rows re-classified)
3. **Multi-clause parsing**: Token matches the pattern but text after the match (`、` or `。`) is discarded, or `し、` prefix stripping doesn't lead to successful re-match
4. **Output format divergence**: Token produces different English wording than CSV expects
5. **Regex missing variations**: AssistPowerBoostToken doesn't handle `《NIKKE》` trait insertion → **RC-AE** (1 row re-classified)
6. **Condition prefix blocking**: `あなたのターン中、` prefix before ability pattern → **RC-AG** (1 row re-classified)

**Phase 1 Re-classification Breakdown**:
- **RC-AD (multi-clause/cost parsing)**: 15 test rows (12 cost tokens + 3 CONT ability tokens)
- **RC-AE (regex/output mismatch)**: 2 test rows (NIK/S117-091 trait variation + ANM/W138-T11 wording)
- **RC-AG (condition chain)**: 1 test row (NIK/S117-015 prefix)
- **✅ Fixed**: 1 test row (ANM/W138-T12 AssistPowerBoostToken regression)
- [x] **RC-F1**: Created `PlacedFromHandPowerBoostToken` (Phase 2, item 14)
  - **Token**: `PlacedFromHandPowerBoostToken.cs`
  - **Status**: ✅ CREATED - Registered
- [x] **RC-G1**: Created `BattleOpponentLevelConditionToken` (Phase 3, item 16)
  - **Token**: `BattleOpponentLevelConditionToken.cs`
  - **Status**: ✅ CREATED - Registered
- [x] **RC-G2**: Created `PutCharacterToBottomOfOpponentDeckToken` (Phase 3, item 17 variant)
  - **Token**: `PutCharacterToBottomOfOpponentDeckToken.cs`
  - **Status**: ✅ CREATED - Registered
- [x] **RC-H1**: Created `SearchLevelXOrLowerTraitToken` (Phase 3, item 18)
  - **Token**: `SearchLevelXOrLowerTraitToken.cs`
  - **Status**: ✅ CREATED - Registered
- [x] **RC-H2**: Created `RevealTopCardIfTraitAddToHandToken` (Phase 3, item 19)
  - **Token**: `RevealTopCardIfTraitAddToHandToken.cs`
  - **Status**: ✅ CREATED - Registered
- [x] **RC-I1**: Created `TriggerCheckTwoTimesToken` (Phase 3, item 20)
  - **Token**: `TriggerCheckTwoTimesToken.cs`
  - **Status**: ✅ CREATED - Registered
- [x] **RC-I2**: Created `PutClockToWrOrStockToken` (Phase 3, item 21)
  - **Token**: `PutClockToWrOrStockToken.cs`
  - **Status**: ✅ CREATED - Registered
- [x] **RC-K**: Created `ExchangeLevelWithWrToken` (RC-K item 1)
  - **Token**: `ExchangeLevelWithWrToken.cs`
  - **Status**: ✅ CREATED - Registered
- [x] **RC-K**: Created `DealDamageXTimesToken` (RC-K item 3)
  - **Token**: `DealDamageXTimesToken.cs`
  - **Status**: ✅ CREATED - Registered
- [x] **RC-K**: Created `PutOpponentClockToWrToken` (RC-K item 4)
  - **Token**: `PutOpponentClockToWrToken.cs`
  - **Status**: ✅ CREATED - Registered
- [x] **RC-K**: Created `DrawPhaseStartConditionToken` (RC-K item 7)
  - **Token**: `DrawPhaseStartConditionToken.cs`
  - **Status**: ✅ CREATED - Registered
- [x] **RC-K**: Created `RestStandCharacterToken` (RC-K item 8)
  - **Token**: `RestStandCharacterToken.cs`
  - **Status**: ✅ CREATED - Registered
- [x] **RC-K**: Created `DealXDamageToken` (RC-K item 9)
  - **Token**: `DealXDamageToken.cs`
  - **Status**: ✅ CREATED - Registered
- [x] **RC-K**: Created `SearchLevel0OrLowerToken` (RC-K item 10)
  - **Token**: `SearchLevel0OrLowerToken.cs`
  - **Status**: ✅ CREATED - Registered
- [x] **RC-K**: Created `PutTopXCardsToWrToken` (RC-K item 11)
  - **Token**: `PutTopXCardsToWrToken.cs`
  - **Status**: ✅ CREATED - Registered
- [x] **RC-K**: Created `NoColorCardsInLevelConditionToken` (RC-K item 12)
  - **Token**: `NoColorCardsInLevelConditionToken.cs`
  - **Status**: ✅ CREATED - Registered
- [x] **RC-K**: Created `RestIfCxExistsToken` (RC-K item 6)
  - **Token**: `RestIfCxExistsToken.cs`
  - **Status**: ✅ CREATED - Registered
- [x] **RC-K**: Created `CannotUseActUntilEndOfTurnToken` (RC-K item 2)
  - **Token**: `CannotUseActUntilEndOfTurnToken.cs`
  - **Status**: ✅ CREATED - Registered

## In Progress / Needs Fixing

### Phase 1 Re-classified Items
- [ ] **RC-AG**: NIK/S117-015 row 1 — `あなたのターン中、` prefix before AssistPowerBoostToken pattern
- [ ] **RC-AE**: NIK/S117-091 row 1 — AssistPowerBoostToken regex needs `《NIKKE》` trait variation
- [ ] **RC-AD-5**: NIK/S117-078, 099 row 1 — PowerBoostPerOtherNikkeToken exists but `Match` returns null (non-zero index, prefix not skipped)
- [ ] **RC-AD-5**: NIK/S117-052 row 1 — PowerBoostPerOpponentRestToken exists but `Match` returns null (non-zero index, prefix not skipped)
- [ ] **RC-AD-2**: NIK/S117-076 row 1 — SoulBoostOneToken not matching after `し、` prefix strip (needs `し、` in lead-in prefixes)
- [ ] **RC-AD-3**: NIK/S117-044 row 2, NIK/S117-060 row 3, NIK/S117-069 row 2 — CostPutTraitCharacterFromHandToWaitingRoomToken not matched in GetMatch cost context
- [ ] **RC-AD-3**: NIK/S117-108 row 2 — CostRestStandNikkeCharacterToken not matched in GetMatch cost context
- [ ] **RC-AD-3**: NIK/S117-068 row 2 — CostRestTwoNikkeCharactersToken not matched in GetMatch cost context
- [ ] **RC-AD-3**: NIK/S117-063 row 2 — CostPutToStockAndSwapBottomToken not matched in GetMatch cost context
- [x] **RC-AH**: ANM/W138-T12 — ✅ FIXED in Phase 1 (AssistPowerBoostToken fixed-vs-variable distinction)
- [ ] **RC-AE**: ANM/W138-T11 — Output format: "to the bottom" vs "at the bottom"

### Sprint 2 Completed (late 2026-05-18 session)
- [x] **DrawAndDiscardToken**: Created token for `あなたはN枚引いてよい。そうしたら、...控え室に置く。` pattern ✅
  - **Fix**: Regex matches draw-and-discard pattern; output uses singular/plural "card"/"cards" and "it"/"them"
  - **Impact**: NIK/S117-011 row 1 (+1 test)
- [x] **StrikerAbilityToken**: Created token for `大活躍` keyword ✅
  - **Output**: `"Great Performance"`
  - **Impact**: NIK/S117-017 row 3 (+1 test, part of +3 total for that serial)
- [x] **CannotBeChosenAbilityToken**: Created ability-level token for nested `『』` contexts ✅
  - **Output**: `"This card cannot be chosen by your opponent's effects."`
  - **Fix**: Condition token matched with "If " prefix in nested context; ability token bypasses condition processing
- [x] **Power boost regex fixes**:
  - `SimplePowerBoostToken`: Added `(?:を)?` for `パワー(を)＋` and `[XＸ\d]` for variable X ✅
  - `PowerBoostToken`: Added `[XＸ\d]` for `Ｘ` character class ✅
- [x] **Continuative verb ending fixes**:
  - `CostPutCxFromHandToWaitingRoomToken`: `置(?:く|き)` ✅
  - `CostPutTraitCharacterFromHandToWaitingRoomToken`: Same ✅
  - `PutCardFromHandToWaitingRoomToken`: Added `、` to trailing punctuation ✅
- [x] **ChooseFromWR return-to-deck**: Made `まで` optional; singular/plural fixes ✅
  - **Impact**: NIK/S117-015 (+1 test)
- [x] **ChooseFromWR return**: Added "you may" detection via `てよい` ✅
  - **Impact**: NIK/S117-005 (+2 tests)
- [x] **TokenRegistry.Match**: Debug logging added for index-0 traces ✅
- [x] **TryTranslateNested refactored** (`GainFollowingAbilityToken.cs`) ✅
  - **Problem**: `ContEffectToken` (in `EffectRegistry`) matched nested `【永】` text with "If " prefix via `ParseSentence`
  - **Fix**: Split `TryMatchAny` → `TryMatchEffect` + `TryMatchAbility`; process order: lead-in strip → effect match (with `【】`) → strip `【】` → ability match → `ParseSentence`
  - **Impact**: NIK/S117-017 (+3 tests), preserved NIK/S117-006 passing
- [x] **GainEncoreAbilityToken**: Added `[XＸ\d]` for variable X support ✅

### Phase 1.5 Completed (2026-05-18 session)
- [x] **RC-AD-2**: ActEffectToken cost parsing — replaced `GetMatch` with `Match` loop ✅
- [x] **RC-AD-6**: ActEffectToken abilities — replaced `TryFindFirstMatch` with `MultiClauseEffectParser.Parse` ✅
- [x] **RC-AD Brainstorm (no icon)**: Created `ForEachCxToken` for `それらのカードの CX1 枚につき` pattern ✅
- [x] **Crash guards**: `AutoEffectToken` and `ContEffectToken` — two-level `remaining` check ✅
- [x] **ContEffectToken TokenLog**: Populated from parsed conditions/abilities ✅
- [x] **Cost separator**: Stock-cost-aware joining (space after `(N)`, ` & ` between actions) in `AutoEffectToken` + `ActEffectToken` ✅
- [x] **RC-AE**: Fixed `PutCharacterToBottomOfOpponentDeckToken` output format ("at" → "to") ✅
- [x] **RC-AD-5**: Fixed `ChooseOtherCharacterAndGiveAbilityToken` regex — all groups optional to handle prefix variations ✅
  - **Impact**: Fixed NIK/S117-006 (nested ability grant after `そうしたら、`)
- [x] **RC-H2**: Created `ChooseTraitCharacterFromWrAndPutToStockToken` ✅
  - **Impact**: Fixed NIK/S117-008 row 1 (choose trait character from WR and put to stock)
- [x] **RC-H2**: Extended `ChooseFromWaitingRoomAndReturnToken` to handle CX with trigger icons ✅
  - **Impact**: Fixed NIK/S117-008 row 2 (choose CX with [CHOICE] from WR and return to hand)
  - **Fix**: Strip `.gif` extension from trigger icon, remove redundant "may" (already in cost)

### Original Remaining Items
- [ ] **RC-A1 (regression)**: AssistPowerBoostToken matches fixed numbers — needs fixed-vs-variable distinction
- [ ] **RC-A3**: PowerBoostPerOpponentRestToken regex not matching — verify whitespace/character width
- [ ] **RC-B1**: `CostPutTraitCharacterFromHandToWaitingRoomToken` regex needs fixing
- [ ] **RC-C2**: `GiveEncoreToOpponentCharactersToken` exists but not matching "相手のキャラすべてに"
- [ ] **RC-D1**: `CxNamedPlacedConditionToken` quote style handling
- [ ] **RC-E1**: Treasure icon reminder text translation
- [ ] **RC-F2**: `LookAtTopCardsToken` follow-up action handling
- [ ] **RC-J1**: `ActEffectToken` cost parsing extension
- [ ] **RC-K**: `MoveToOpenPositionToken` already exists but not matching
- [ ] **RC-K**: `OpponentCannotPlayEventsToken` already exists but output format mismatch
- [ ] **RC-L**: Sub-ability granting — nested `『』` ability translation
- [ ] **RC-M**: Give ability to all characters
- [ ] **RC-N**: "All players perform" pattern
- [ ] **RC-O**: Level-dependent multi-clause effect
- [ ] **RC-P**: "OR" combined trigger condition (`時か時`)
- [ ] **RC-Q**: "When playing from hand" CONT condition
- [ ] **RC-R**: "Damage not canceled" condition
- [ ] **RC-S**: Cost — put colored character at bottom of clock
- [ ] **RC-T**: Deck count condition (`枚以下なら`)
- [ ] **RC-U**: "CX among revealed cards" conditional
- [ ] **RC-V**: Complex CONT AND/OR conjunction style
- [ ] **RC-W**: "Level equal to or lower than" search
- [ ] **RC-X**: Move opponent character open position
- [ ] **RC-Y**: "When put to WR from stage" trigger
- [ ] **RC-Z**: "No other trait" negative condition
- [ ] **RC-AA**: "When you use backup" effect
- [ ] **RC-AB**: "Cannot play events/backup" grant
- [ ] **RC-AC**: effects_550.csv cross-expansion coverage
- [ ] **RC-AD**: Multi-clause ability parsing (ARCHITECTURAL — biggest impact, ~40+ failures)
  - **AD-0**: Entity restructuring — `AbilityPrefix` + `ConditionConjunction` enums
  - **AD-0.5**: Token registry unification — `TokenMatch Match()` only returns index-0 matches, logs skipped text via Serilog for non-zero matches, `TokenLog` debug logging
  - **AD-1**: Extract `MultiClauseEffectParser` — sentence splitting + iterative parsing + prefix/conjunction tracking
  - **AD-2**: Fix cost token matching — `Match` loop with strict index-0 enforcement (costs = sequential `CardEffectAbility` matches, no prefix skipping) — ✅ DONE in ActEffectToken
  - **AD-3**: Lead-In Prefix Map — typed prefix→enum mapping + recursive skipping
  - **AD-4**: Two-pass `Match` — index-0 first, then earliest
  - **AD-5**: Enhance nested `『』` ability translation
  - **AD-6**: Integrate — replace duplicated logic in all effect tokens — ✅ DONE in ActEffectToken; still needed in CounterEffectToken, GainFollowingAbilityToken
- [ ] **RC-AE**: CSV expected value / output format mismatches
- [ ] **RC-AF**: Top-level parsing gap (no 能力 prefix)
- [ ] **RC-AG**: Complex chain conditions with で/て/く connectors
- [ ] **RC-AH**: AssistPowerBoostToken fixed-vs-variable regression

## Key Insight (2026-05-18)
- `ThoseCardsTriggerIconConditionToken` needs `MultiClauseEffectParser` integration: its `ParseTokenList` uses legacy `TryFindFirstMatch` and skips `CX1枚につき、` prefix via character-eating, losing the "For each CX" semantics for the trigger-icon variant. Fix: replace `ParseTokenList` with `MultiClauseEffectParser.ParseSentence`.
- `ForEachCxToken` (no trigger icon) works correctly via `MultiClauseEffectParser.Parse` sentence splitting.
- Cost separator logic: stock cost `(N)` uses space separator; action costs use ` & `. Applied in both `AutoEffectToken` and `ActEffectToken`.

## Notes
- Many failures cascade: fixing one token may unblock 5+ serials
- CSV fixes should be done AFTER code fixes to avoid rework
- Some serials have multiple issues (e.g., NIK/S117-105 has RC-C2 + RC-G1)
- Priority order based on impact (number of serials fixed per token)
- **Key insight**: Most remaining failures are due to multi-clause ability parsing, not missing tokens
- **Critical finding**: The majority of "created" tokens (24/24) are NOT passing tests due to the above issues
- **Revalidation count**: Plan baseline said 96 serials / 151 failures — actual is 97 serials / 148 failures (slightly improved but 2 serials from effects_550.csv were unaccounted)
- **RC-AD detailed plan**: See RC-AD section above for 8 failure groups (AD-1 through AD-8) and 8 implementation sub-phases (AD-0 through AD-0.5, AD-1 through AD-6). Full architecture analysis in `.plans/RC-AD_Comprehensive_Fix_Plan.md`.
- **Entity restructuring first**: AD-0 adds `AbilityPrefix` and `ConditionConjunction` enums — must be done before AD-1. Default values ensure zero regressions.
- **Registry unification**: AD-0.5 merges 3 match methods into `TokenMatch Match()`, adds `TokenLog` for debugging, enforces strict matching with index warnings. Must be done before AD-1.
- **Quick win**: Adding `し、` → `Continuation` to prefix map (AD-3) fixes ~8 tests independently of other RC-AD work
- **Do NOT create new tokens for RC-AD** — the problem is the parsing pipeline, not missing tokens
