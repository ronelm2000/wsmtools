# RC-AD: Multi-Clause Ability Parsing — Comprehensive Fix Plan

## Status: In Progress (as of 2026-05-18 13:55)

**Test Results**: 115 passed, 131 failed, 246 total (CSV cross-check)
**Full Suite**: 693 passed, 172 failed, 865 total (TestCategory!=Manual)
**Build**: Succeeded (0 errors)
**Branch**: master (47 commits ahead of origin)
**Latest Commit**: 08e716b - Add condition and ability tokens for NIK/S117 series
**AD-5 Status**: RC-5.1 fixed. RC-5.5 fixed. RC-5.2 fixed. RC-5.6 fixed. Both NIK/S117-064 effects pass. NIK/S117-002 passes.

### Recent Progress
- ✅ **AutoEffectToken fully refactored** — delegates to `MultiClauseEffectParser.ParseSentence` for multi-clause parsing
- ✅ **MayPayCostThenAbilityToken refactored** — uses `ParseSentence` instead of legacy `TryFindFirstMatch`
- ✅ **Cost parsing uses `Match` API** — `ParseCostText` replaces `GetMatch`
- ✅ **Debug logging added** — `Log.Debug` in `AutoEffectToken`, `ActEffectToken`, `ContEffectToken`, and `MultiClauseEffectParser` for parsing diagnostics
- ✅ **NIK/S117-110 now passes** — fixed `RestStandCharacterToken` (no-space Japanese text), `DealDamageToken` (conditional X def), `XEqualsToken` (soul case), opponent reference regex
- ✅ **AGENTS.md updated** — added Logging section with standard pattern
- ✅ **`AbilityLeadInPrefixes` removed** from `AutoEffectToken` — replaced by `MultiClauseEffectParser.DefaultPrefixMap` + `SkippablePrefixes`
- ✅ **`JoinAbilityPartsWithPrefix` added** — groups abilities by prefix type for proper sentence formatting
- ✅ **ActEffectToken refactored** — now uses `ParseCostText` + `MultiClauseEffectParser.ParseSentence` (replaces `GetMatch` + `TryFindFirstMatch`)
- ✅ **ContEffectToken refactored** — now uses `MultiClauseEffectParser.ParseSentence` (replaces `TryMatchAtStart` + `TryFindFirstMatch` + `ConditionalLeadInPrefixes`)
- ✅ **AutoEffectToken sentence splitting** — uses `MultiClauseEffectParser.Parse` (with `。` splitting) instead of `ParseSentence`
- ✅ **MayPayCost pattern protected** — `コストを払ってよい。そうしたら` is protected from `。` splitting in `MultiClauseEffectParser.Parse`
- ✅ **JoinAbilityPartsWithPrefix enhanced** — detects abilities starting with `If`/`When`/`During`/`At` as sentence boundaries
- ✅ **Fallback matching added** — if prefix skipping fails, `ParseSentence` falls back to matching original text (fixes tokens like `GiveEncoreToOpponentCharactersToken` that include `相手の` in regex)
- ✅ **ACT-7/CONT-8: Subject prefix fix complete** — removed `あなたの`, `相手の`, `他の`, `このカードは`, `このカードが`, `次の` from `ParseSentence` subject prefix skip list (keeping only `あなたは` and `自分の`). Fixed all CONT regressions where token regexes included these prefixes (e.g., `AllCharactersBoostToken`, `AllCharactersSoulBoostToken`, `AssistPowerBoostToken`, etc.)
- ✅ **CSV cross-check stable** — 91 passed; working on improving pass rate
- ✅ **`MultiClauseEffectParser.Parse` sentence split protection** — protects `『』`, `〔〕`, `コストを払ってよい。`, `[ＸＹXY]は...に等しい。`, `（...）` from `。` splitting
- ✅ **`ParseSentence` direct-match-first + prefix fallback** — tries exact index-0 match before stripping skippable prefixes, preserving tokens with `^あなたの`, `^相手の` etc.
- ✅ **`AssistContEffectToken` merged into `ContEffectToken`** — multi-label support (`応援`, `経験`, `記憶`) without fragile separate token
- ✅ **`AssistPowerBoostToken` trait regex + `×` symbol** — handles `《NIKKE》の` trait variants, outputs `×` instead of `x`
- ✅ **`GiveEncoreToOpponentCharactersToken` regex fix** — fixed `？` → proper regex, now matches in registry
- ✅ **Punctuation handling for `get the following abilities`** — `ContEffectToken` skips adding `.` when ability text contains `"get the following abilities"` (nested abilities handle own punctuation)
- ✅ **AD-5 Root Cause Analysis complete** — 9 distinct root causes identified across ~26 failures. Key finding: `AutoEffectToken` still uses legacy parsing loop at lines 133-197 despite plan claiming it was refactored.
- ✅ **RC-5.1: AutoEffectToken fully refactored** (commit 74d9806) — replaced legacy `TryMatchAtStart`/`TryFindFirstMatch` loop at lines 133-197 with `MultiClauseEffectParser.ParseSentence`. Cost parsing also uses `Match` API loop.
- ✅ **RC-5.5: Opponent reference post-processing removed** (commit c18bbfc) — removed blunt regex replacement that caused "your opponent's opponent" and "their next turn" issues. Tokens now handle their own opponent references correctly.
- ✅ **RC-5.2: Duration prefix added to PowerBoostWithFollowingAbilityToken** (commit c18bbfc) — handles `そのターン中、このカードのパワーを＋Nし、このカードは次の能力を得る。『...』` pattern with duration tracking.
- ✅ **Both NIK/S117-064 effects now pass** — first effect (cost + power + opponent events) and second effect (attack + power + following ability) both translate correctly.
- ✅ **Condition prefix stripping** — Stripped "When ", "If ", "During ", "At " prefixes from all condition token `ConditionText` values. Now defers to `Type` property for prefix generation via `AggregateToString`.
- ✅ **`AggregateToString` space fix** — Fixed `string.Join(",", parts)` → `string.Join(", ", parts)` for proper spacing.
- ✅ **New tokens added**: `PowerBoostGainEncoreToken`, `CannotMoveToAnotherPositionToken`, `LevelBoostToken`, `DealFixedDamageToken`, `ClockToWaitingRoomSimpleToken`
- ✅ **More new tokens added**: `SearchDeckSimpleToken`, `ChooseFromWaitingRoomAndReturnToDeckToken`, `OtherCharacterCountConditionToken`, `CardWithMarkerPlacedToWaitingRoomFromStageConditionToken`, `ReturnThisCardToStageAsRestToken`, `CardPlacedFromHandOrMemoryConditionToken`, `PutCharacterToClockToken`, `PutThisCardToMemoryToken`
- ✅ **`GainEncoreAbilityToken` fixed** — Uses `Match` API instead of deprecated `GetMatch`
- ✅ **`ChooseFromWaitingRoomAndReturnToken` fixed** — Added "you may" prefix for optional actions
- ✅ **`RestStandCharacterToken` fixed** — Handles no-space Japanese text (`レベル3以下のキャラ`)
- ✅ **`XEqualsToken` fixed** — Added soul case handling (`そのキャラのソウル`)
- ✅ **`ClockToWaitingRoomToken` fixed** — Handles `まで` (up to) and verb variants (`いてよい`, `き`, `く`)
- ✅ **`ActEffectToken` duplication bug fixed** — Removed duplicate ability appending in effect text construction
- ✅ **`ActEffectToken` spacing fixed** — No extra space between `[ACT]` and cost bracket
- ✅ **`PutCardFromHandAndThisToBottomToken` moved** — Registered before `PutCardFromHandToWaitingRoomToken` (registration order)
- ✅ **`NoFacingCharacterOrReversedConditionToken` moved** — Registered before `FacingCharacterColorConditionToken`
- ✅ **`CxNamedPlacedConditionToken` regex fixed** — Added optional `(?:あなたの)?` prefix
- ✅ **New tokens added**: `SearchDeckSimpleToken`, `ChooseFromWaitingRoomAndReturnToDeckToken`, `OtherCharacterCountConditionToken`, `CardWithMarkerPlacedToWaitingRoomFromStageConditionToken`, `ReturnThisCardToStageAsRestToken`, `CardPlacedFromHandOrMemoryConditionToken`, `PutCharacterToClockToken`, `PutThisCardToMemoryToken`
- ✅ **`ContEffectToken` now uses `AggregateToString`** — Removed duplicate condition prefix handling
- ✅ **Phase start condition types fixed** — `CxPhaseStartConditionToken`, `EncoreStepStartConditionToken`, `DrawPhaseStartConditionToken` changed from `When` to `At`
- ✅ **Condition `ConditionText` prefix stripping** — Removed "if " from `FacingCharacterColorConditionToken`, `FacingCharacterLevelConditionToken`, `FacingOpponentCharacterConditionToken`, `OpponentLevelConditionToken`, `DuringTurnFacingCharacterColorConditionToken`, `CenterStageConditionToken`
- ✅ **`GainFollowingAbilityToken` comma fix** — Removed extra comma in "power, and the following ability"
- ✅ **`CannotPlayBackupDuringBattleToken` word order fixed** — "During this card's battle, your opponent cannot..."
- ✅ **`GainEncoreAbilityToken` capitalization fixed** — Cost text now properly capitalized
- ✅ **`EncoreStepStartConditionToken` regex fixed** — Handles both `あなたの` and bare `アンコールステップ`
- ✅ **`MultiAssert.cs` syntax fix** — Added missing closing brace
- ✅ **CSV cross-check improved** — 115 passed (up from 91 baseline)

### Phase Completion Summary

| Phase | Status | Notes |
|-------|--------|-------|
| AD-0: Entity Restructuring | ✅ Complete | `AbilityPrefix` enum in `CardEffectAbility.cs`, `ConditionConjunction` enum in `CardEffectCondition.cs` |
| AD-0.5: Registry Unification | ✅ Complete | `GetMatch` removed (7c3accf), `TokenLog` added to effect tokens (a734401), unified `Match` in place |
| AD-1: Multi-Clause Parser | ✅ Complete | `MultiClauseEffectParser.cs` created with `Parse`, `ParseSentence`, `DetectLeadInPrefix`, `SkippablePrefixes`, sentence split protection |
| AD-2: Cost Token Matching | ✅ Complete | `ParseCostText` in `AutoEffectToken` replaces `GetMatch` with `Match` loop |
| AD-3: Prefix Map + Recursive Skipping | ✅ Complete | `DefaultPrefixMap` + `SkippablePrefixes` in `MultiClauseEffectParser.cs` |
| AD-4: Match Priority | ✅ Complete | Handled by AD-0.5 — `Match` enforces index-0 only |
| AD-5: Nested Ability Translation | ⚠️ Partial | RC-5.1 fixed (AutoEffectToken refactored). RC-5.5 fixed (opponent reference post-processing removed). RC-5.2 fixed (duration prefix added to PowerBoostWithFollowingAbilityToken). RC-5.6 fixed (MayPayCostThenAbilityToken). Both NIK/S117-064 effects pass. |
| AD-6: Integration + Refactoring | ✅ Complete | `AutoEffectToken`, `ActEffectToken`, `ContEffectToken` all delegate to `ParseSentence`/`Parse` |
| **NEW: Conditional Ability Token** | ✅ Working | `ConditionalAbilityToken` handles `[condition]なら、[ability]` patterns (NIK/S117-100 passes) |
| **NEW: AutoEffectToken Refactor** | ✅ Complete (460fe66) | Delegates to `ParseSentence`, uses `Match` API, debug logging, `JoinAbilityParts` |
| **NEW: Token Regex Fixes + Punctuation** | ✅ Complete (f5912d8) | `GiveEncoreToOpponentCharactersToken` regex, `AssistPowerBoostToken` trait/`×`, `AssistContEffectToken` merged, `ContEffectToken` punctuation handling |
| **NEW: Registration Order Fixes** | ✅ Complete | `NoFacingCharacterOrReversedConditionToken` before `FacingCharacterColorConditionToken`, `PutCardFromHandAndThisToBottomToken` before `PutCardFromHandToWaitingRoomToken` |
| **NEW: Condition Prefix Stripping** | ✅ Complete | All condition tokens defer to `Type` for prefix generation via `AggregateToString` |
| **NEW: Phase Start Type Fixes** | ✅ Complete | `CxPhaseStartConditionToken`, `EncoreStepStartConditionToken`, `DrawPhaseStartConditionToken` changed from `When` to `At` |
| **NEW: ActEffectToken Fixes** | ✅ Complete | Fixed duplication bug, spacing, and `[ACT][cost]` format |

### Remaining Failure Categories (from 131 failures)

1. **AD-5: Missing "duration + power + gain" token** — ~2 failures (NIK/S117-087) — `PowerBoostWithFollowingAbilityToken` lacks `そのターン中、` prefix variant
2. **AD-5: `TryMatchAny` uses `TryFindFirstMatch`** — ~2 failures (NIK/S117-060, 065) — nested ability translation uses deprecated API
3. **AD-5: Output format mismatches** — ~2 failures (NIK/S117-066, 093) — comma placement, "during" position
4. **AD-5: CXCOMBO condition parsing** — ~1 failure (NIK/S117-087) — Japanese text leaking in output
5. **AD-5: Wrong token matched** — 1 failure (NIK/S117-105) — separate root cause, not nested ability
6. **Sentence boundary (`。`) not splitting** — ~10 failures (other serials)
7. **Continuation `し、` not handled** — ~6 failures
8. **Condition chains with `で`/`て` connectors** — ~12 failures
9. **Unrecognized ability patterns** — ~60+ failures (various — missing tokens or regex issues)
10. **Minor spacing/capitalization** — ~15 failures (trailing periods, extra spaces, etc.)

---

## Cross-Check with Comprehensive Fix Plan

Cross-referenced with `.plans/Comprehensive_Fix_Plan_CSV_CrossCheckAll.md` (baseline: 100/246, 148 failing rows).

### Out-of-Scope Items (Not RC-AD)

These items from the RC-AD plan's remaining failures are **not multi-clause parsing issues** and belong to other root cause categories:

| RC-AD Item | Actual RC | Reason |
|------------|-----------|--------|
| AD-5: Wrong token matched (NIK/S117-105) | RC-C2 | Token matching issue, not nested ability |
| Unrecognized ability patterns (~60+) | RC-A through RC-K, RC-M through RC-Z | Missing tokens or regex gaps, not parsing pipeline |
| Minor spacing/capitalization (~15) | RC-AE | Output format mismatches, CSV alignment |

### Resolved by Architectural Changes (Since Comprehensive Plan Baseline)

These items from the comprehensive plan are **now fixed** by RC-AD architectural changes:

| Comprehensive Plan Item | RC-AD Fix | Status |
|------------------------|-----------|--------|
| RC-A1: AssistPowerBoostToken regex | Token fix + RC-AD multi-clause | ✅ Fixed |
| RC-A2: PowerBoostPerOtherNikkeToken | Registration order + prefix skipping | ✅ Fixed |
| RC-A3: PowerBoostPerOpponentRestToken | Registration order + prefix skipping | ✅ Fixed |
| RC-A4: SoulBoostOneToken | `し、` in SkippablePrefixes | ✅ Fixed |
| RC-B1 through RC-B4: Cost tokens | `Match` API replaces `GetMatch` | ✅ Fixed |
| RC-C1: GainEncoreAbilityToken | Created + `Match` API | ✅ Fixed |
| RC-D1: CxNamedPlacedConditionToken | Regex `(あなたの)?` prefix | ✅ Fixed |
| RC-F1: PlacedFromHandPowerBoostToken | Created | ✅ Fixed |
| RC-G1: BattleOpponentLevelConditionToken | Created | ✅ Fixed |
| RC-H1: SearchLevelXOrLowerTraitToken | Created | ✅ Fixed |
| RC-H2: RevealTopCardIfTraitAddToHandToken | Created | ✅ Fixed |
| RC-I1: TriggerCheckTwoTimesToken | Created | ✅ Fixed |
| RC-I2: PutClockToWrOrStockToken | Created | ✅ Fixed |
| RC-J1: ActEffectToken cost parsing | Duplication bug fixed | ✅ Fixed |
| RC-K: Various tokens (ExchangeLevel, DealDamageX, etc.) | Created | ✅ Fixed |
| RC-AH: AssistPowerBoostToken regression | Fixed vs variable X distinction | ✅ Fixed |

### Needs Re-Analysis Due to Architectural Changes

These items from the comprehensive plan **need re-evaluation** because RC-AD architectural changes altered their root cause:

| Comprehensive Plan Item | Change Impact | Re-Analysis Needed |
|------------------------|---------------|-------------------|
| RC-AE: Output format mismatches | Condition prefix stripping changed `ConditionText` format; `AggregateToString` now handles prefix generation. Many "output mismatch" failures may now be fixed or have different root causes. | Check each RC-AE failure against new condition formatting |
| RC-D2: Memory card exists condition | `CardExistsInMemoryConditionToken` created but still fails due to multi-clause CXCOMBO chain. Now an RC-AD issue. | Re-classified to RC-AD |
| RC-F2: LookAtTopCardsToken follow-up | Token exists but follow-up action discarded. Multi-clause issue. | Re-classified to RC-AD |
| RC-G2: ReverseCharacterOptionalToken | Token created but not matching after prefix strip. Multi-clause issue. | Re-classified to RC-AD |
| RC-L: Sub-ability granting | `GainFollowingAbilityToken` and `PowerBoostWithFollowingAbilityToken` enhanced. `TryTranslateNested` still uses `TryFindFirstMatch`. | Partially fixed, RC-5.4 remaining |
| RC-V: Complex CONT AND/OR conjunction | `AggregateToString` now handles conjunction grouping. May be fixed. | Verify |
| RC-AB: Cannot play events/backup | `CannotPlayBackupDuringBattleToken` word order fixed. CONT embedding may still fail. | Partially fixed |
| RC-AG: Complex chain conditions | Condition tokens now atomic; `AggregateToString` groups by type. Chain parsing improved but `で`/`て` connectors may still need work. | Partially fixed |

### Key Finding: Outdated in Both Plans

Both plans state: **"AutoEffectToken still uses legacy parsing loop at lines 133-197 with TryMatchAtStart/TryFindFirstMatch"**

**This is now FALSE.** `AutoEffectToken` has been fully refactored:
- Uses `MultiClauseEffectParser.ParseSentence` for condition + ability parsing
- Uses `Match` API for cost parsing (no more `GetMatch`)
- No `TryMatchAtStart`, `TryFindFirstMatch`, or `GetMatch` calls remain
- Debug logging added

### Remaining Work (Aligned with Both Plans)

| Priority | Item | Est. Impact | Notes |
|----------|------|-------------|-------|
| High | AD-5.4: `TryMatchAny` uses `TryFindFirstMatch` | ~2 failures | Replace with `Match` API in `GainFollowingAbilityToken` |
| High | AD-5.2: Duration prefix variant | ~2 failures | Add `そのターン中、` to `PowerBoostWithFollowingAbilityToken` |
| High | AD-5.9: CXCOMBO condition parsing | ~1 failure | Japanese text leaking — condition token not matching |
| Medium | Sentence boundary (`。`) splitting | ~10 failures | `AutoEffectToken` uses `ParseSentence` not `Parse` |
| Medium | Continuation `し、` handling | ~6 failures | Already in `SkippablePrefixes` but may not cover all cases |
| Medium | Condition chains with `で`/`て` | ~12 failures | Overlaps with RC-AG — condition tokens need connector variants |
| Low | Output format alignment (RC-AE) | ~15 failures | CSV updates or token wording adjustments |
| Low | Missing tokens (RC-M through RC-Z) | ~20 failures | New tokens needed, not RC-AD scope |

---


## Overview

**RC-AD** is the single largest root cause category (~40+ failures). It is **architectural**, not a missing token issue. Individual tokens exist for most sub-patterns, but the parsing pipeline stops at the first matched ability clause and discards the rest.

### Core Problem

Japanese abilities have the structure: `[condition]、[action1]、[action2]、[action3]。` or use continuative connectors like `し、` and `て、`. The current parser:

1. Matches one ability token
2. Consumes the matched text + strips trailing punctuation
3. Loops to find the next ability
4. **Breaks** when it encounters unrecognized text (prefix not in `AbilityLeadInPrefixes`, or no token matches)

### Why Other Effect Tokens Succeed

| Token | Sentence Splitting on `。` | `し` Handling | Prefix Skipping |
|-------|---------------------------|---------------|-----------------|
| `EventEffectToken` | ✅ Yes | ❌ No | ❌ No (breaks) |
| `BrainstormEffectToken` | ✅ Yes | ❌ No | ❌ No (char skip) |
| `AutoEffectToken` | ❌ No | ❌ No | ✅ Yes (limited) |
| `ActEffectToken` | ❌ No | ❌ No | ❌ No (breaks) |
| `ContEffectToken` | ❌ No | ✅ Yes | ❌ No (throws) |

---

## Failure Pattern Groups

### Group AD-1: Sentence Boundary (`。`) Not Split — ~12 failures

**Pattern**: Multiple sentences separated by `。` within a single effect. Parser processes as one continuous stream and fails when the second sentence doesn't match at index 0.

**Examples**:
- NIK/S117-046: `手札に戻す。...控え室に置いてよい。そうしたら、...手札に戻す` (3 sentences)
- NIK/S117-014: `手札に加える。手札に加えたなら、...` (2 sentences, second has conditional)
- NIK/S117-065: `相手に1ダメージを与える。この能力を使うには...` (2 sentences)

**Affected Effect Types**: `AutoEffectToken`, `ActEffectToken`, `ContEffectToken`

**Fix**: Add sentence splitting (already done in `EventEffectToken`/`BrainstormEffectToken`) to all three effect tokens.

---

### Group AD-2: Continuation Particle `し、` Not Handled — ~8 failures

**Pattern**: Abilities connected by `し、` (continuative form meaning "and also"). Only `ContEffectToken` handles this via `ConditionalLeadInPrefixes`.

**Examples**:
- NIK/S117-076: `パワーを＋2000し、ソウルを＋1。` (power boost + soul boost)
- NIK/S117-011: `パワーを＋1000し、次の能力を与える。『【自】...』` (power boost + give ability)
- NIK/S117-052: `見て...選び...置き...し、使えない` (5+ actions connected by `し、`)

**Affected Effect Types**: `AutoEffectToken`, `ActEffectToken`

**Fix**: Add `し、` and `し` to `AbilityLeadInPrefixes` in `AutoEffectToken`. Add similar prefix array to `ActEffectToken`.

---

### Group AD-3: Cost Token Matching via `GetMatch` — ~7 failures

**Pattern**: Cost text extracted from `［...］` and passed to `EffectListRegistry.GetMatch()`. `GetMatch` requires an **exact match at the start** and throws if no token matches. Cost tokens exist but `GetMatch` fails because:

1. Cost text may have leading/trailing whitespace variations
2. Cost text may contain multiple clauses separated by `、`
3. `GetMatch` uses `Where(token => token.Matcher.IsMatch())` then takes first — but if the regex doesn't match the full string, it fails

**Examples**:
- NIK/S117-044: `［手札の《NIKKE》のキャラを1枚控え室に置く］` — cost is a full ability clause
- NIK/S117-068: `［あなたの《NIKKE》のキャラを2枚【レスト】する］` — cost with count
- NIK/S117-108: `［他のあなたの【スタンド】している《NIKKE》のキャラを1枚【レスト】する］` — cost with condition

**Affected Files**: `AutoEffectToken.cs:92-94`, `ActEffectToken.cs:25-27`

**Fix**: Replace `GetMatch` with `Match` loop with strict index-0 enforcement.

---

### Group AD-4: Lead-In Prefix Exhaustion — ~6 failures

**Pattern**: After matching an ability, the remaining text starts with a prefix not in `AbilityLeadInPrefixes`. Parser skips known prefixes once, then **breaks** if the text after the prefix still doesn't match.

**Current prefixes** (`AutoEffectToken.AbilityLeadInPrefixes`):
```
["あなたは", "あなたの", "自分の", "そうしたら、", "そうしたら", 
 "その後、", "その後", "次の", "そして、", "そして"]
```

**Missing prefixes**:
- `そうしなければ、` / `そうしなければ` ("if you don't")
- `そうでなければ、` / `そうでなければ` ("otherwise")
- `このカードは` / `このカードが` ("this card")
- `あなたは...してよい` optional pattern
- `て、` (continuative verb form, different from `し、`)
- `ながら` ("while")
- `ために` ("in order to")

**Examples**:
- NIK/S117-052: `相手の【レスト】しているキャラ...` — starts with `相手の` which is not a lead-in
- NIK/S117-092: `他のあなたの《NIKKE》のキャラが3枚以上で...` — starts with `他の`

**Fix**: Expand `AbilityLeadInPrefixes` and add recursive prefix skipping (skip prefix, retry match, skip again if needed).

---

### Group AD-5: `Match` Strictness — Index-0 Only — ~5 failures

**Pattern**: `TokenRegistry.Match` only returns index-0 matches. Tokens matching at index > 0 are not returned — instead a warning is logged with skipped text. General token at index 5 is ignored in favor of no match, forcing prefix skipping or regex fix.

**Examples**:
- NIK/S117-078: `PowerBoostPerOtherNikkeToken` exists but `Match` returns null (matched at non-zero index)
- NIK/S117-099: Same — token exists but registry doesn't find it at index 0

**Fix**: `Match` enforces index-0 only. Callers must skip lead-in prefixes before calling `Match`, or fix token regex to anchor at `^`.

---

### Group AD-6: Nested Ability in `『』` Not Fully Translated — ~4 failures

**Pattern**: Abilities that grant sub-abilities use `『【自】...』` or `『【永】...』` format. `PowerBoostWithFollowingAbilityToken.TryTranslateNested()` handles this but has limitations:

1. Only triggered by specific parent tokens (e.g., `PowerBoostWithFollowingAbilityToken`)
2. `GainFollowingAbilityToken` captures the nested text but relies on `TryTranslateNested` which may not handle all patterns
3. Nested abilities may themselves be multi-clause

**Examples**:
- NIK/S117-060: `次の能力を得る。『【自】...手札に戻す。...控え室に置く』` — nested multi-clause
- NIK/S117-064: `次の能力を得る。『【永】相手はイベントを手札からプレイできない』` — nested CONT
- NIK/S117-102: Similar nested ability grant

**Fix**: Make `TryTranslateNested` more robust by applying sentence splitting and prefix handling within nested context.

---

### Group AD-7: Condition Chain with `で`/`て`/`く` Connectors — ~4 failures

**Pattern**: Multiple conditions chained with particles before the final `なら`:
```
CX置場に「...」があり、前列にこのカードがいて、...が2枚以上で、...がいないなら
```

Each segment is a separate condition, but the parser tries to match as a single pattern. The condition loop in `AutoEffectToken` (lines 114-127) iteratively matches conditions from the start, but:

1. `TryMatchAtStart` requires the condition token regex to match at index 0
2. After matching one condition, it strips `、` and continues
3. If a condition token's regex doesn't account for the `で` connector variant, it fails

**Examples**:
- NIK/S117-002: `あり、いて、...で、...なら` (4+ chained conditions)
- NIK/S117-030: Similar chain with CX + position + count conditions

**Fix**: This overlaps with RC-AG but is also an RC-AD issue. Condition tokens need `で`/`て` variant patterns, OR the condition parser needs to split on `、` and match each segment independently.

---

### Group AD-8: Cascading Optional Actions (`そうしたら` / "If You Do") — ~3 failures

**Pattern**: Optional action followed by cascading effect:
```
手札に戻す。...控え室に置いてよい。そうしたら、...手札に戻す
```

The `そうしたら` ("if you do" / "then") is a lead-in prefix that gets skipped, but the text after it may contain another conditional + ability chain that the parser cannot handle.

**Examples**:
- NIK/S117-046: `手札に戻す。...置いてよい。そうしたら、...戻す`
- NIK/S117-069: Similar cascade

**Fix**: After skipping `そうしたら`, the parser should re-enter the full condition + ability parsing loop, not just try ability matching.

---

## Entity Restructuring: Structured Prefix & Conjunction Tracking

Current lead-in prefixes are **silently discarded** during parsing. They should instead be captured as structured data so the output formatter can produce correct English conjunctions.

### `CardEffectAbility` — Add `Prefix` Property

**File**: `Montage.Weiss.Tools/Entities/Effect/CardEffectAbility.cs`

```csharp
public enum AbilityPrefix
{
    /// <summary>Default. Actions joined by comma: "do X, do Y, do Z."</summary>
    And,
    /// <summary>`し、` / `て、` — continuative "and also". Same output as And but semantically distinct.</summary>
    Continuation,
    /// <summary>`そうしたら` / `そうすれば` — "if you do" / "then". Output: "If you do, ..."</summary>
    IfYouDo,
    /// <summary>`そうでないなら` / `そうでなければ` / `そうしなければ` — "otherwise" / "if not". Output: "Otherwise, ..."</summary>
    Otherwise,
    /// <summary>`その後` — "after that" / "then". Output: "After that, ..."</summary>
    AfterThat,
    /// <summary>`あなたは` / `自分の` — subject prefix with no special conjunction. Output: "you ..."</summary>
    Subject,
}

public record CardEffectAbility
{
    public required string AbilityText { get; init; }
    public AbilityPrefix Prefix { get; init; } = AbilityPrefix.And;

    public static CardEffectAbility operator +(CardEffectAbility a, CardEffectAbility b)
    {
        var connector = b.Prefix switch
        {
            AbilityPrefix.IfYouDo => ". If you do, ",
            AbilityPrefix.Otherwise => ". Otherwise, ",
            AbilityPrefix.AfterThat => ". After that, ",
            AbilityPrefix.Continuation => ", and ",
            AbilityPrefix.Subject => " ",
            _ => ", ", // And (default)
        };
        return new CardEffectAbility
        {
            AbilityText = $"{a.AbilityText}{connector}{char.ToLower(b.AbilityText[0]) + b.AbilityText[1..]}"
        };
    }
}
```

### `CardEffectCondition` — Add `Conjunction` Property

**File**: `Montage.Weiss.Tools/Entities/Effect/CardEffectCondition.cs`

```csharp
public enum ConditionConjunction
{
    /// <summary>Default. Conditions joined by "and": "if X, and Y, and Z".</summary>
    And,
    /// <summary>`か` / `または` — "or". Output: "if X or Y".</summary>
    Or,
    /// <summary>`で` — continuative "and" for conditions. Same output as And but semantically distinct.</summary>
    Continuation,
}

public record CardEffectCondition
{
    public required ConditionType Type { get; init; }
    public required string ConditionText { get; init; }
    public ConditionConjunction Conjunction { get; init; } = ConditionConjunction.And;
}
```

### How Prefixes Map to Japanese Patterns

| Japanese Pattern | AbilityPrefix | Output Example |
|-----------------|---------------|----------------|
| *(none / default)* | `And` | `choose 1 character, put it face up` |
| `し、` / `し` / `て、` / `て` | `Continuation` | `get +2000 power, and get +1 soul` |
| `そうしたら、` / `そうしたら` | `IfYouDo` | `put it in your hand. If you do, draw 1 card` |
| `そうでないなら、` / `そうでなければ、` | `Otherwise` | `reveal the top card. Otherwise, shuffle it` |
| `そうしなければ、` / `そうしなければ` | `Otherwise` | `choose a card. Otherwise, put it in WR` |
| `その後、` / `その後` | `AfterThat` | `shuffle your deck. After that, draw 1 card` |
| `あなたは` / `自分の` / `このカードは` | `Subject` | `you may choose 1 character` |
| `相手の` / `他の` | `Subject` | `your opponent's character gets +1000` |

### How Conjunctions Map to Japanese Patterns

| Japanese Pattern | ConditionConjunction | Output Example |
|-----------------|---------------------|----------------|
| `、` (between conditions) | `And` | `if you have 3 cards, and it is your turn` |
| `で` (continuative) | `Continuation` | `if you have 3 cards, and it is your turn` |
| `か` / `または` | `Or` | `if you have no cards or your hand is empty` |

### Example: Full Parsing Flow

**Input**: `あなたは自分の控え室の《NIKKE》のキャラを1枚選び、このカードの下にマーカーとして表向きに置いてよい。`

**Parsed abilities**:
1. `CardEffectAbility { AbilityText: "choose 1 <<NIKKE>> character in your waiting room", Prefix: Subject }` (from `あなたは...選び`)
2. `CardEffectAbility { AbilityText: "put it face up underneath this card as a marker", Prefix: And }` (from `このカードの下に...置いてよい`)

**Output**: `you may choose 1 <<NIKKE>> character in your waiting room, and put it face up underneath this card as a marker.`

**Input**: `このカードは、あなたのレベル置場に、黄のカードと赤のカードと青のカードがあるなら、このカードは、色条件を満たさずに手札からプレイできる。そうでないなら、手札からプレイできない。`

**Parsed** (ignoring `このカードは、` lead-in):
1. `CardEffectCondition { ConditionText: "your level has a yellow card, a red card and a blue card", Type: If, Conjunction: And }`
2. `CardEffectAbility { AbilityText: "this card can be played from your hand without satisfying its color condition", Prefix: And }`
3. `CardEffectAbility { AbilityText: "this card cannot be played from your hand", Prefix: Otherwise }`

**Output**: `If your level has a yellow card, a red card and a blue card, this card can be played from your hand without satisfying its color condition. Otherwise, this card cannot be played from your hand.`

### Parser Responsibility

The `MultiClauseEffectParser` (or effect tokens during iterative parsing) must:

1. **Detect the lead-in prefix** before each ability match
2. **Map it to an `AbilityPrefix`** enum value
3. **Pass it to the ability token** or set it on the resulting `CardEffectAbility`
4. **Detect condition conjunctions** (`で`, `か`, `または`) between conditions
5. **Set `ConditionConjunction`** on each `CardEffectCondition`

Two approaches for passing prefix to tokens:

**Approach A**: Parser sets `Prefix` on the returned `CardEffectAbility` after token translation.
```csharp
var abilList = abilFunc(registry);
foreach (var abil in abilList)
{
    abil.Prefix = detectedPrefix; // Set after translation
}
```

**Approach B**: Parser wraps the token's translate function to inject prefix.
```csharp
// In Match or the ability loop
var wrappedFunc = (ITokenRegistry reg) => {
    var abils = abilFunc(reg);
    return abils.Select(a => a with { Prefix = detectedPrefix }).ToList();
};
```

**Approach A is simpler** and requires fewer changes to the registry system.

---

## Implementation Plan

### Phase AD-0: Entity Restructuring (Prerequisite) — ✅ COMPLETE

**Status**: Implemented. `AbilityPrefix` enum and `Prefix` property added to `CardEffectAbility.cs`. `ConditionConjunction` enum and `Conjunction` property added to `CardEffectCondition.cs`. `+` operator for conjunction-based joining implemented.

**Files Modified**:
- `CardEffectAbility.cs` — `AbilityPrefix` enum + `Prefix` property + `+` operator
- `CardEffectCondition.cs` — `ConditionConjunction` enum + `Conjunction` property

---

### Phase AD-0.5: Token Registry Unification + Debug Logging (Prerequisite) — ✅ COMPLETE

**Status**: Implemented. `GetMatch(string)` removed (commit 7c3accf). `TokenLog` population added to Auto, Act, and Cont effect tokens (commit a734401). Unified `Match` method enforces index-0 only with diagnostic warnings.

**Files Modified**:
- `TokenRegistry.cs` — replace `GetMatch`, `TryMatchAtStart`, `TryFindFirstMatch` with unified `Match`
- `CardTextToken.cs` — add `IComponentRegistry<E>.Match` to interface
- `CardEffect.cs` (or base effect record) — add `List<string> TokenLog { get; init; } = [];`
- `AutoEffectToken.cs`, `ActEffectToken.cs`, `ContEffectToken.cs`, `EventEffectToken.cs`, `BrainstormEffectToken.cs` — update to use new `Match` API
- All ability/condition tokens — update to use new `Match` API

**New Types**:
```csharp
public record TokenMatch(
    ReadOnlyMemory<char> Input,
    int Index,
    int Length,
    string Token); // Class name of the matched token

public record TokenMatchResult<E>(
    TokenMatch Match,
    Func<ITokenRegistry, E> Translate);
```

**New `IComponentRegistry<E>` Interface**:
```csharp
public interface IComponentRegistry<E>
{
    void Register(CardTextToken<E> token);
    IEnumerable<CardTextToken<E>> GetAllTokens();

    /// <summary>
    /// Matches input against all registered tokens. Only returns matches at index 0.
    /// </summary>
    /// <remarks>
    /// Strictness: Tokens matching at Index &gt; 0 are NOT returned. Instead, a warning
    /// is logged via Serilog.Log showing what text would be skipped and which tokens
    /// matched mid-string. This enforces that all parsing consumes from the start.
    /// Callers must handle null by either throwing or skipping known lead-in prefixes.
    /// </remarks>
    TokenMatchResult<E>? Match(ReadOnlyMemory<char> input);
}
```

**Implementation**:
```csharp
public TokenMatchResult<E>? Match(ReadOnlyMemory<char> input)
{
    TokenMatch? bestAtZero = null;
    Func<ITokenRegistry, E>? bestTranslate = null;
    List<(string Token, int Index)> nonZeroMatches = [];

    foreach (var token in _tokens)
    {
        var match = token.Matcher.Match(input.Span);
        if (!match.Success) continue;

        if (match.Index == 0)
        {
            // Index-0 match: registration order preserved, return first
            var span = input.Slice(match.Index, match.Length);
            bestAtZero = new TokenMatch(input, 0, match.Length, token.GetType().Name);
            bestTranslate = registry => token.Translate(registry, span);
            break; // First registration wins at index 0
        }

        // Track non-zero matches for diagnostic logging
        nonZeroMatches.Add((token.GetType().Name, match.Index));
    }

    if (bestAtZero != null)
        return new TokenMatchResult<E>(bestAtZero.Value, bestTranslate!);

    // No index-0 match found — log diagnostic warning about skipped text
    if (nonZeroMatches.Count > 0)
    {
        Log.Warning(
            "No token matched at index 0. {Count} token(s) matched mid-string. " +
            "Skipped text: '{Skipped}'. Input: '{Input}'. Matches: {Matches}",
            nonZeroMatches.Count,
            input[..nonZeroMatches.Min(m => m.Index)].ToString(),
            input.ToString(),
            string.Join(", ", nonZeroMatches.Select(m => $"{m.Token}@{m.Index}")));
    }
    else
    {
        Log.Warning("No token matched at all for input: '{Input}'", input.ToString());
    }

    return null;
}
```

**Migration from Old API**:
```csharp
// Before: GetMatch
var func = registry.EffectListRegistry.GetMatch(input);
var result = func(registry);

// After: Match
var matchResult = registry.EffectListRegistry.Match(input);
if (matchResult == null) throw new NotImplementedException($"No token found for: {input}");
var result = matchResult.Translate(registry);
// Log the token
effect.TokenLog.Add(matchResult.Match.Token);

// Before: TryMatchAtStart
if (registry.ConditionListRegistry.TryMatchAtStart(input, out var func, out var consumed)) { ... }

// After: Match (already enforces index-0)
var matchResult = registry.ConditionListRegistry.Match(input.AsMemory());
if (matchResult != null)
{
    var consumed = matchResult.Match.Length;
    var func = matchResult.Translate;
    // ...
}

// Before: TryFindFirstMatch
if (registry.EffectListRegistry.TryFindFirstMatch(input, out var func, out var idx, out var len)) { ... }

// After: Match (only returns index-0, logs skipped text for non-zero)
var matchResult = registry.EffectListRegistry.Match(input.AsMemory());
if (matchResult != null)
{
    // matchResult.Match.Index is always 0 here
    var len = matchResult.Match.Length;
    var func = matchResult.Translate;
    // ...
}
// If matchResult is null, check if lead-in prefix should be skipped first
```

**TokenLog Population Strategy**:

Each effect token, after matching sub-tokens (abilities, conditions, costs), appends to the parent effect's `TokenLog`:

```csharp
// In AutoEffectToken.Translate():
var effect = new AutoCardEffect { TokenLog = new List<string>() };

// When matching cost:
var costMatch = registry.EffectListRegistry.Match(costTextJapanese.AsMemory());
if (costMatch != null)
{
    effect.TokenLog.Add($"Cost:{costMatch.Match.Token}");
    // ...
}

// When matching conditions:
var condMatch = registry.ConditionListRegistry.Match(trimmed.AsMemory());
if (condMatch != null && condMatch.Match.Index == 0)
{
    effect.TokenLog.Add($"Cond:{condMatch.Match.Token}");
    // ...
}

// When matching abilities:
var abilMatch = registry.EffectListRegistry.Match(trimmed.AsMemory());
if (abilMatch != null)
{
    effect.TokenLog.Add($"Abil:{abilMatch.Match.Token}");
    // ...
}
```

**TokenLog Output Example**:
```
[
  "Effect:AutoEffectToken",
  "Cost:CostPutTraitCharacterFromHandToWaitingRoomToken",
  "Cond:CxNamedPlacedConditionToken",
  "Abil:SearchDeckToken",
  "Abil:PutToHandToken",
  "Abil:IfYouDoToken",
  "Abil:DrawToken"
]
```

This makes debugging trivial — you can see exactly which tokens matched and in what order.

**Impact**: 
- `Match` only returns index-0 matches — non-zero index matches return `null` with diagnostic warning
- Warning includes skipped text, which tokens matched mid-string, and at which indices
- `TokenLog` is debug-only, not used for serialization or business logic
- All existing tokens must anchor regex at `^` or be updated to do so
- Callers handle `null` by either throwing or skipping known lead-in prefixes before retrying

**Tests**: Verify no regressions in 100+ currently passing tests. Verify `TokenLog` is populated correctly for a sample of effects.

---

### Phase AD-1: Extract Shared Multi-Clause Parser (Foundation) — ✅ COMPLETE

**Status**: `MultiClauseEffectParser.cs` created with `Parse`, `ParseSentence`, `DetectLeadInPrefix`, `DefaultPrefixMap`, `SkippablePrefixes`, and sentence split protection for `『』`, `〔〕`, `コストを払ってよい。`, `[ＸＹXY]は...に等しい。`, `（...）`. `ParseSentence` uses direct-match-first + prefix fallback strategy.

**New File**: `Montage.Weiss.Tools/Entities/Effect/Token/MultiClauseEffectParser.cs`

**Responsibilities**:
1. Split input on `。` (protecting `『』` content)
2. For each sentence:
   a. Iteratively match conditions from start (`TryMatchAtStart`), tracking `Conjunction` between them
   b. Detect condition conjunctions (`で` → `Continuation`, `か` → `Or`) from trailing punctuation/particles
   c. Iteratively match abilities (`Match`), detecting lead-in prefix before each match
   d. Map detected prefix to `AbilityPrefix` and set on resulting `CardEffectAbility`
   e. Handle continuation particles (`し、`, `て、`, `し`, `て`) → `Continuation`
3. Join results using `Prefix`/`Conjunction` values (not hardcoded punctuation)

**Methods**:
```csharp
public static class MultiClauseEffectParser
{
    // Main entry point — splits on 。 and parses each sentence
    public static ParsedEffect Parse(
        string input,
        ITokenRegistry registry,
        LeadInPrefixMap? prefixMap = null);

    // Parse a single sentence (no 。 splitting)
    public static ParsedSentence ParseSentence(
        string sentence,
        ITokenRegistry registry,
        LeadInPrefixMap? prefixMap = null);

    // Detect lead-in prefix at start of input, return prefix type + remaining text
    public static (AbilityPrefix Prefix, string Rest) DetectLeadInPrefix(string input, LeadInPrefixMap? map = null);

    // Detect condition conjunction from trailing particle/punctuation
    public static ConditionConjunction DetectConditionConjunction(string input, out string stripped);
}

public record LeadInPrefixMap(
    IReadOnlyDictionary<string, AbilityPrefix> Prefixes,
    IReadOnlyDictionary<string, AbilityPrefix>? Fallbacks = null);

public record ParsedEffect(
    List<CardEffectCondition> Conditions,
    List<CardEffectAbility> Abilities,
    string ConditionText,
    string AbilityText);

public record ParsedSentence(
    List<CardEffectCondition> Conditions,
    List<CardEffectAbility> Abilities,
    string Text);
```

**Prefix Detection Logic**:
```csharp
public static (AbilityPrefix Prefix, string Rest) DetectLeadInPrefix(string input, LeadInPrefixMap? map = null)
{
    map ??= DefaultPrefixMap;
    foreach (var (pattern, prefix) in map.Prefixes)
    {
        if (input.StartsWith(pattern, StringComparison.Ordinal))
            return (prefix, input[pattern.Length..].TrimStart('、', ' ', '\t'));
    }
    return (AbilityPrefix.And, input); // Default: no prefix detected
}
```

**Condition Conjunction Detection**:
```csharp
public static ConditionConjunction DetectConditionConjunction(string input, out string stripped)
{
    // Check for OR particle: か or または before なら
    if (Regex.IsMatch(input, @"か[^な]*なら$") || input.Contains("または"))
    {
        stripped = Regex.Replace(input, @"(か|または)", "、"); // Normalize to 、 for parsing
        return ConditionConjunction.Or;
    }
    // Check for continuative で before next condition
    if (input.EndsWith("で、") || input.EndsWith("で"))
    {
        stripped = input.TrimEnd('で', '、');
        return ConditionConjunction.Continuation;
    }
    stripped = input;
    return ConditionConjunction.And;
}
```

**Files Modified**:
- `AutoEffectToken.cs` — delegate to `MultiClauseEffectParser.Parse()`
- `ActEffectToken.cs` — delegate to `MultiClauseEffectParser.Parse()`
- `ContEffectToken.cs` — delegate to `MultiClauseEffectParser.Parse()` (remove duplicate logic)
- `EventEffectToken.cs` — optionally delegate (already has sentence splitting)
- `BrainstormEffectToken.cs` — optionally delegate

**Tests**: Verify no regressions in currently passing tests (100+).

---

### Phase AD-2: Fix Cost Token Matching — ⚠️ PARTIAL

**Status**: Cost parsing uses `Match` but still failing for complex costs. NIK/S117-108 produces empty ability after "If you do, .", NIK/S117-110 has untranslated Japanese text (`レベル3以下の`, `そのキャラのソウル`). Cost token ambiguity warnings still present (`CostRestTraitCharactersToken` vs `CostRestStandNikkeCharacterToken`).

**Files Modified**:
- `AutoEffectToken.cs:92-94` — change `GetMatch` to `Match` loop with strict index-0
- `ActEffectToken.cs:25-27` — same change

**Approach**:
```csharp
// Before:
var costAbilities = registry.EffectListRegistry.GetMatch(costTextJapanese.AsMemory())(registry);

// After:
var costAbilities = new List<CardEffectAbility>();
var costRemaining = costTextJapanese;
while (!string.IsNullOrWhiteSpace(costRemaining))
{
    var t = costRemaining.TrimStart();
    var matchResult = registry.EffectListRegistry.Match(t.AsMemory());
    if (matchResult == null) break;

    // Strict: cost tokens MUST match at index 0
    if (matchResult.Match.Index > 0)
    {
        throw new NotImplementedException(
            $"Cost token '{matchResult.Match.Token}' matched at index {matchResult.Match.Index} instead of 0. " +
            $"Remaining cost text: '{t}'. Unmatched prefix: '{t[..matchResult.Match.Index]}'");
    }

    var abils = matchResult.Translate(registry);
    costAbilities.AddRange(abils);
    costRemaining = t[matchResult.Match.Length..].TrimStart('、', ' ', '\t');
}
if (costAbilities.Count == 0)
    throw new NotImplementedException($"No cost token found for: {costTextJapanese}");
```

**Example**: Cost `[(1) ［このカードを【レスト】する］ 山札の上から1枚をクロック置場に置く]` parses as:
1. `Match` → `(1) cost` at index 0, consumes `(1)`
2. `Match` → `CostRestThisCardToken` at index 0, consumes `このカードを【レスト】する`
3. `Match` → `PutTopCardToClockToken` at index 0, consumes `山札の上から1枚をクロック置場に置く`

**Tests**: Verify cost parsing for NIK/S117-044, 060, 068, 069, 108.

---

### Phase AD-3: Lead-In Prefix Map + Recursive Skipping — ✅ COMPLETE

**Status**: `DefaultPrefixMap` with 20+ prefixes implemented in `MultiClauseEffectParser.cs`. Includes continuation (`し、`, `て、`), if-you-do (`そうしたら`), otherwise (`そうでなければ`), after-that (`その後`), and subject prefixes. `DetectLeadInPrefix` method working.

**Key Distinction**:
- **Costs**: Strict index-0 matching. Each cost token must match at the start of remaining text. No prefix skipping allowed. If a cost token doesn't match at index 0, it's an error.
- **Abilities**: Prefix detection + skipping. Lead-in prefixes (`し、`, `そうしたら`, etc.) are detected, mapped to `AbilityPrefix`, and the remaining text is matched. The prefix value is stored on the resulting `CardEffectAbility`.

**Files Modified**:
- `MultiClauseEffectParser.cs` — `DetectLeadInPrefix` with `LeadInPrefixMap`
- `AutoEffectToken.cs` — remove `AbilityLeadInPrefixes` array, use `DefaultPrefixMap` for ability loop
- `ActEffectToken.cs` — add prefix detection via `MultiClauseEffectParser.DetectLeadInPrefix`

**Default Prefix Map**:
```csharp
public static readonly LeadInPrefixMap DefaultPrefixMap = new(new Dictionary<string, AbilityPrefix>
{
    // Continuation (し、/て、) → Continuation
    { "し、", AbilityPrefix.Continuation },
    { "し", AbilityPrefix.Continuation },
    { "て、", AbilityPrefix.Continuation },
    { "て", AbilityPrefix.Continuation },
    // If you do (そうしたら) → IfYouDo
    { "そうしたら、", AbilityPrefix.IfYouDo },
    { "そうしたら", AbilityPrefix.IfYouDo },
    // Otherwise (そうでないなら/そうでなければ/そうしなければ) → Otherwise
    { "そうでないなら、", AbilityPrefix.Otherwise },
    { "そうでないなら", AbilityPrefix.Otherwise },
    { "そうでなければ、", AbilityPrefix.Otherwise },
    { "そうでなければ", AbilityPrefix.Otherwise },
    { "そうしなければ、", AbilityPrefix.Otherwise },
    { "そうしなければ", AbilityPrefix.Otherwise },
    // After that (その後) → AfterThat
    { "その後、", AbilityPrefix.AfterThat },
    { "その後", AbilityPrefix.AfterThat },
    // Subject prefixes (no special conjunction) → Subject
    { "あなたは", AbilityPrefix.Subject },
    { "あなたの", AbilityPrefix.Subject },
    { "自分の", AbilityPrefix.Subject },
    { "このカードは", AbilityPrefix.Subject },
    { "このカードが", AbilityPrefix.Subject },
    { "相手の", AbilityPrefix.Subject },
    { "他の", AbilityPrefix.Subject },
    // Then/and (そして) → Continuation
    { "そして、", AbilityPrefix.Continuation },
    { "そして", AbilityPrefix.Continuation },
    // Next (次の) → Subject
    { "次の", AbilityPrefix.Subject },
});
```

**Recursive Skipping**:
```csharp
// In ability loop — detect prefix, set on ability, retry match
while (!string.IsNullOrWhiteSpace(remainingText))
{
    var (prefix, rest) = MultiClauseEffectParser.DetectLeadInPrefix(remainingText.TrimStart());
    if (prefix != AbilityPrefix.And)
    {
        remainingText = rest;
        // Fall through to next iteration — Match will find the token,
        // then parser sets abil.Prefix = prefix on the result
    }
    else
    {
        // No prefix detected — try matching ability directly
        var matchResult = registry.EffectListRegistry.Match(trimmed.AsMemory());
        if (matchResult != null) { /* matched */ }
        else { break; } // truly unrecognized
    }
}
```

---

### Phase AD-4: Match Priority (Handled by AD-0.5) — ✅ COMPLETE

**Status**: Handled by AD-0.5. The unified `Match` method enforces index-0 only with diagnostic warnings for non-zero matches.

---

### Phase AD-5: Enhance Nested Ability Translation — ⚠️ PARTIAL (Root Cause Analysis Complete)

**Status**: Root cause analysis complete. 9 distinct root causes identified across ~22 CSV entries with nested `『』` patterns. RC-5.1, RC-5.2, RC-5.3, RC-5.5, RC-5.6, RC-5.8, RC-5.9 resolved. RC-5.4 and RC-5.7 remaining.

**Root Cause Analysis** (as of 2026-05-18 03:45):

| RC ID | Category | Affected Serials | Count | Description |
|-------|----------|-----------------|-------|-------------|
| RC-AD-5.1 | `AutoEffectToken` uses legacy parsing | NIK/S117-064, 065, 087, 101 | ~8 | `AutoEffectToken.cs` lines 133-197 still uses `TryMatchAtStart`/`TryFindFirstMatch` instead of `MultiClauseEffectParser.ParseSentence`. Breaks on `そのターン中、このカードのパワーを＋Nし、このカードは次の能力を得る。『...』` patterns. |
| RC-AD-5.2 | Missing "duration + power + gain" token | NIK/S117-064, 065, 087 | ~4 | `PowerBoostWithFollowingAbilityToken` regex lacks `そのターン中、` prefix variant. Pattern: `そのターン中、このカードのパワーを＋Nし、このカードは次の能力を得る。『...』` |
| RC-AD-5.3 | Missing "power + gain encore" token | NIK/S117-066, 093 | ~2 | No token handles `このカードのパワーを＋Nし、このカードは『【自】 アンコール ［...］』を得る。` — `し、` connector between power boost and encore gain not matched. |
| RC-AD-5.4 | `TryMatchAny` uses deprecated `TryFindFirstMatch` | NIK/S117-060, 064, 065, 102 | ~4 | `GainFollowingAbilityToken.cs` lines 95, 109 use `TryFindFirstMatch` which finds matches at any index, causing incorrect token selection for nested abilities. |
| RC-AD-5.5 | Output format mismatches | NIK/S117-066, 093, 102 | ~3 | Comma placement (`gets +4500 power, and` vs `gets +4500 power and`), "during" position, "and" vs ". This card gets". |
| RC-AD-5.6 | MayPayCostThenAbilityToken drops ability | NIK/S117-066 | ~2 | After cost parsing, ability after `そうしたら` is not matched. Produces `If you do, .` (empty). |
| RC-AD-5.7 | Wrong token matched (not nested ability) | NIK/S117-105 | 1 | "return it to your hand" instead of "put it face up underneath this card as a marker". Separate root cause — token matching issue. |
| RC-AD-5.8 | Pluralization | NIK/S117-093 | 1 | "character" vs "characters" — minor wording issue. |
| RC-AD-5.9 | CXCOMBO condition parsing (Japanese leaking) | NIK/S117-087 | 1 | `If CX置場に「OVER ZONE」が置かれた時、前列にこのカードがいる,` — CXCOMBO conditions not fully parsed. |

**Key Finding**: ~~The plan states `AutoEffectToken` was "fully refactored" (commit 460fe66) to delegate to `ParseSentence`, but the actual code at `AutoEffectToken.cs:133-197` still contains the legacy parsing loop with `TryMatchAtStart` and `TryFindFirstMatch`.~~ **RESOLVED** — `AutoEffectToken` now fully delegates to `MultiClauseEffectParser.ParseSentence`, uses `Match` API for costs, and has no legacy API calls. The single largest blocker is now `TryMatchAny` in `GainFollowingAbilityToken` still using `TryFindFirstMatch`.

**Total AD-5 impact**: ~22 failures across 9 root causes (some serials have multiple effects). Down from ~26.

---

### Phase AD-6: Integration + Refactoring — ✅ COMPLETE

**Status**: All effect tokens delegate to `MultiClauseEffectParser`:
1. **AutoEffectToken** — ✅ Fully delegates to `MultiClauseEffectParser.ParseSentence()`. Uses `Match` API for costs. Debug logging added. `JoinAbilityParts` for proper sentence formatting.
2. **ActEffectToken** — ✅ Delegates to `ParseSentence` with `ParseCostText` and debug logging. Fixed duplication bug and spacing.
3. **ContEffectToken** — ✅ Delegates to `ParseSentence`, uses `AggregateToString` for condition formatting, multi-label support (`応援`, `経験`, `記憶`), punctuation handling for nested abilities.
4. **Sentence splitting on `。`** — ✅ Wired into `MultiClauseEffectParser.Parse` with protection for `『』`, `〔〕`, etc.
5. **`Prefix`/`Conjunction` values** — ✅ Detected and applied in `JoinAbilityParts`
6. **Token regex fixes** — ✅ `GiveEncoreToOpponentCharactersToken`, `AssistPowerBoostToken`, `AssistContEffectToken` merged (commit f5912d8)
7. **Registration order fixes** — `NoFacingCharacterOrReversedConditionToken` before `FacingCharacterColorConditionToken`, `PutCardFromHandAndThisToBottomToken` before `PutCardFromHandToWaitingRoomToken`

---

## Testing Strategy

### Per-Phase Testing

```powershell
# After each phase:
dotnet build
dotnet test --filter "FullyQualifiedName~Translate_CSV_CrossCheckAll" --no-build

# Also run full test suite to catch regressions:
dotnet test --filter "TestCategory!=Manual" --no-build
```

### Expected Impact per Phase

| Phase | Estimated Tests Fixed | Risk Level | Status |
|-------|----------------------|------------|--------|
| AD-0: Entity Restructuring | +0 (infrastructure) | Low | ✅ Done |
| AD-0.5: Registry Unification | +0 (infrastructure) | Medium | ✅ Done |
| AD-1: Multi-Clause Parser | +15-20 | Medium | ✅ Complete |
| AD-2: Cost Token Matching | +7 | Low | ✅ ParseCostText replaces GetMatch |
| AD-3: Prefix Expansion | +8-10 | Low | ✅ Done |
| AD-4: Match Priority Fix | +5 | Low | ✅ Done (by AD-0.5) |
| AD-5: Nested Ability Fix | +26 | Medium | ⚠️ RCA complete. RC-5.1, RC-5.2, RC-5.5, RC-5.6 fixed. Both NIK/S117-064 effects pass. Remaining: duration prefix variants, TryMatchAny migration, output format. |
| AD-6: Integration | +5-10 (overlap with AD-1) | High | ✅ Complete (f5912d8) |
| Token Regex Fixes | +9 | Low | ✅ Complete (f5912d8) |

**Current**: 115 passed / 246 total (47% pass rate)
**Target**: ~140-155 passing (57-63% pass rate)

### Regression Guardrails

1. **Before any changes**: Record baseline of 115 passing tests
2. **After each phase**: Verify no previously-passing tests regress
3. **Token registry tests**: Run `Registry_*` tests to ensure token validation still passes
4. **Manual spot checks**: Verify output format for NIK/S117-002, 046, 052, 060, 065

---

## Risk Assessment

### High Risk
- **AD-1 + AD-6**: Refactoring core parsing logic could break all effect translation
  - Mitigation: Extract `MultiClauseEffectParser` as a separate class first, test it independently, then integrate
  - Mitigation: Keep existing logic as fallback during transition

### Medium Risk
- **AD-0.5**: Changing `Match` to only return index-0 matches could affect token matching across the board
  - Mitigation: Add unit tests for specific token priority scenarios
  - Mitigation: Log warnings when behavior changes

### Low Risk
- **AD-2**: Cost parsing change is isolated to cost extraction
- **AD-3**: Prefix expansion is purely additive
- **AD-5**: Nested ability fix only affects `『』` content

---

## Dependencies

- **RC-AG** (complex chain conditions): Overlaps with AD-7. Fix together or ensure condition tokens handle `で`/`て` variants.
- **RC-AE** (output format mismatch): Some RC-AD fixes may change output format. Update CSV after RC-AD is stable.
- **RC-L** (sub-ability granting): Depends on AD-5 (nested ability translation).
- **RC-O** (level-dependent branching): Depends on AD-1 (sentence splitting) + new token for level branches.

---

## Success Criteria

- [x] `AbilityPrefix` enum + `Prefix` property added to `CardEffectAbility`
- [x] `ConditionConjunction` enum + `Conjunction` property added to `CardEffectCondition`
- [x] `GetMatch` removed from `IComponentRegistry<E>` — only `Match` remains
- [x] `MultiClauseEffectParser` created with prefix detection, default prefix map, and skippable prefixes
- [x] `TokenLog` populated for Auto, Act, and Cont effect tokens
- [x] `AutoEffectToken` fully delegates to `MultiClauseEffectParser.ParseSentence` (commit 74d9806)
- [x] `MayPayCostThenAbilityToken` uses `ParseSentence` instead of legacy `TryFindFirstMatch`
- [x] Cost parsing uses `Match` API (`ParseCostText` replaces `GetMatch`)
- [x] Debug logging standard established (AGENTS.md updated)
- [x] Opponent reference post-processing removed from `AutoEffectToken` (commit c18bbfc)
- [x] Duration prefix handling added to `ParseSentence` and `PowerBoostWithFollowingAbilityToken`
- [x] Both NIK/S117-064 effects pass
- [ ] `PowerBoostWithFollowingAbilityToken` supports all duration prefix variants
- [ ] Combined "power boost + gain encore" token created for `し、このカードは『...』を得る。` pattern
- [ ] `GainFollowingAbilityToken.TryMatchAny` uses `Match` API instead of `TryFindFirstMatch`
- [ ] Output format: comma placement, "during" position, "and" vs ". This card gets" aligned
- [ ] CXCOMBO condition parsing fixed (no Japanese leaking)
- [ ] All 22 AD-5 failures resolved or reclassified (down from 26)
- [x] 0 regressions in previously passing tests (101 baseline)
- [x] `ActEffectToken` delegates to `MultiClauseEffectParser`
- [x] `ContEffectToken` delegates to `MultiClauseEffectParser`
- [x] Sentence splitting on `。` wired into effect tokens (AutoEffectToken via `Parse`)
- [x] Continuation `し、` handling shared across all effect tokens (via `SkippablePrefixes` in `ParseSentence`)
- [ ] Cost tokens parse correctly for complex multi-clause costs
- [ ] Nested abilities in `『』` fully translated
- [x] `dotnet build` passes with 0 errors
- [ ] Pre-commit hooks pass
- [x] `GiveEncoreToOpponentCharactersToken` regex fixed, matches in registry (f5912d8)
- [x] `AssistContEffectToken` merged into `ContEffectToken` with multi-label support (f5912d8)
- [x] `AssistPowerBoostToken` trait regex + `×` symbol fixed (f5912d8)
- [x] `ContEffectToken` punctuation handling for nested abilities (f5912d8)
- [x] CSV cross-check improved to 115 passed (working towards 140+)
- [x] Condition prefix stripping complete — all condition tokens now defer to `Type` for prefix generation
- [x] `AggregateToString` fixed — proper spacing and lowercase for non-first conditions
- [x] New tokens: `PowerBoostGainEncoreToken`, `CannotMoveToAnotherPositionToken`, `LevelBoostToken`, `DealFixedDamageToken`, `ClockToWaitingRoomSimpleToken`, `SearchDeckSimpleToken`, `ChooseFromWaitingRoomAndReturnToDeckToken`, `OtherCharacterCountConditionToken`, `CardWithMarkerPlacedToWaitingRoomFromStageConditionToken`, `ReturnThisCardToStageAsRestToken`, `CardPlacedFromHandOrMemoryConditionToken`, `PutCharacterToClockToken`, `PutThisCardToMemoryToken`
- [x] `GainEncoreAbilityToken` uses `Match` API (no more `GetMatch`)
- [x] `ChooseFromWaitingRoomAndReturnToken` fixed — "you may" prefix for optional actions
- [x] `ActEffectToken` duplication bug fixed — no more double ability text
- [x] `ActEffectToken` spacing fixed — proper `[ACT][cost]` format
- [x] Registration order fixes — `NoFacingCharacterOrReversedConditionToken`, `PutCardFromHandAndThisToBottomToken`
- [x] `CxNamedPlacedConditionToken` regex fixed — optional `(あなたの)?` prefix
- [x] Phase start condition types fixed — `CxPhaseStart`, `EncoreStepStart`, `DrawPhaseStart` changed from `When` to `At`
- [x] 16 items from Comprehensive Fix Plan resolved by RC-AD architectural changes
- [ ] Cross-check: Verify RC-AE output format mismatches against new condition formatting
- [ ] Cross-check: Verify RC-V complex CONT conjunction style with `AggregateToString`
- [ ] Cross-check: Verify RC-AB CannotPlayBackupDuringBattleToken CONT embedding
- [x] `ActEffectToken` spacing fixed — proper `[ACT][cost]` format
- [x] `ContEffectToken` now uses `AggregateToString` — removed duplicate condition prefix handling
- [x] Phase start condition types fixed — `CxPhaseStartConditionToken`, `EncoreStepStartConditionToken`, `DrawPhaseStartConditionToken` changed from `When` to `At`
- [x] `CxNamedPlacedConditionToken` regex fixed — optional `(あなたの)?` prefix
- [x] Registration order fixes — `NoFacingCharacterOrReversedConditionToken`, `PutCardFromHandAndThisToBottomToken`

---

## File Change Summary

| File | Change Type | Description |
|------|-------------|-------------|
| `AutoEffectToken.cs` | Modify | **Refactor to use `MultiClauseEffectParser.ParseSentence`** — replace legacy `TryMatchAtStart`/`TryFindFirstMatch` loop at lines 133-197 |
| `PowerBoostWithFollowingAbilityToken.cs` | Modify | Add `そのターン中、` prefix variant to regex |
| `GainFollowingAbilityToken.cs` | Modify | Replace `TryFindFirstMatch` in `TryMatchAny` with `Match` API |
| `GainEncoreAbilityToken.cs` or new token | Modify/New | Create combined "power boost + gain encore" token |
| `MayPayCostThenAbilityToken.cs` | Modify | Fix ability dropping after cost payment |
| `CardEffectAbility.cs` | Modify | Add `AbilityPrefix Prefix` enum + property |
| `CardEffectCondition.cs` | Modify | Add `ConditionConjunction Conjunction` enum + property |
| `CardEffect.cs` (base) | Modify | Add `List<string> TokenLog` for debugging |
| `TokenRegistry.cs` | Modify | Merge 3 match methods into `TokenMatch Match()`, strict index warnings |
| `MultiClauseEffectParser.cs` | **NEW** | Shared multi-clause parsing + prefix/conjunction detection + debug logging + sentence split protection |
| `ActEffectToken.cs` | Modify | ✅ Delegate to `ParseSentence`, `ParseCostText`, debug logging. Fixed duplication bug and spacing. |
| `ContEffectToken.cs` | Modify | ✅ Delegate to `ParseSentence`, uses `AggregateToString`, multi-label support, punctuation handling |
| `RestStandCharacterToken.cs` | Modify | Fix no-space Japanese text variants, correct output format |
| `DealDamageToken.cs` | Modify | Conditional X definition based on input text |
| `XEqualsToken.cs` | Modify | Add soul-based X definition case |
| `GiveEncoreToOpponentCharactersToken.cs` | Modify | Fix regex (`？` → proper pattern), now matches in registry |
| `AssistPowerBoostToken.cs` | Modify | Fix trait regex for `《NIKKE》の`, output `×` instead of `x` |
| `AssistContEffectToken.cs` | **DELETE** | Merged into `ContEffectToken` |
| `EventEffectToken.cs` | Optional | Optionally delegate to `MultiClauseEffectParser` |
| `BrainstormEffectToken.cs` | Optional | Optionally delegate to `MultiClauseEffectParser` |
| `AGENTS.md` | Modify | Add Logging section with standard pattern |
| `SearchDeckSimpleToken.cs` | **NEW** | Handles `山札を見て《trait》のキャラをX枚まで選んで相手に見せ` pattern |
| `ChooseFromWaitingRoomAndReturnToDeckToken.cs` | **NEW** | Handles waiting room return to deck + shuffle |
| `OtherCharacterCountConditionToken.cs` | **NEW** | Handles `他のあなたのキャラがN枚以上` condition |
| `CardWithMarkerPlacedToWaitingRoomFromStageConditionToken.cs` | **NEW** | Handles marker + stage→WR condition |
| `ReturnThisCardToStageAsRestToken.cs` | **NEW** | Handles return to stage position as [REST] |
| `CardPlacedFromHandOrMemoryConditionToken.cs` | **NEW** | Handles `手札か思い出置場から舞台に置かれた時` |
| `PutCharacterToClockToken.cs` | **NEW** | Handles `クロック置場に置いてよい` |
| `PutThisCardToMemoryToken.cs` | **NEW** | Handles `このカードを思い出にする` |
| `CxNamedPlacedConditionToken.cs` | Modify | Added optional `(あなたの)?` prefix |
| `PutCardFromHandAndThisToBottomToken.cs` | Modify | Changed separator to `&` |
| `EncoreStepStartConditionToken.cs` | Modify | Regex handles both `あなたの` and bare `アンコールステップ` |
| `ClockToWaitingRoomToken.cs` | Modify | Fixed `まで` (up to) handling |
| `ClockToWaitingRoomSimpleToken.cs` | Modify | Fixed `まで` (up to) handling |
| `CannotPlayBackupDuringBattleToken.cs` | Modify | Fixed word order |
| `GainFollowingAbilityToken.cs` | Modify | Fixed comma in "power, and the following ability" |
| `FacingCharacterColorConditionToken.cs` | Modify | Removed "if " prefix from ConditionText |
| `FacingCharacterLevelConditionToken.cs` | Modify | Removed "if " prefix from ConditionText |
| `FacingOpponentCharacterConditionToken.cs` | Modify | Removed "if " prefix from ConditionText |
| `OpponentLevelConditionToken.cs` | Modify | Removed "if " prefix from ConditionText |
| `DuringTurnFacingCharacterColorConditionToken.cs` | Modify | Removed "if " prefix from ConditionText |
| `CenterStageConditionToken.cs` | Modify | Removed "if " prefix from ConditionText |
| `CxPhaseStartConditionToken.cs` | Modify | Changed type from `When` to `At` |
| `EncoreStepStartConditionToken.cs` | Modify | Changed type from `When` to `At` |
| `DrawPhaseStartConditionToken.cs` | Modify | Changed type from `When` to `At` |
| `MultiAssert.cs` | Modify | Fixed missing closing brace |
| `WeissSchwarzCardTranslatorService.cs` | Modify | Registration order fixes, new token registrations |

---

## Notes

- **Infrastructure phases (AD-0, AD-0.5, AD-1, AD-2, AD-3, AD-4) are complete**. The core types, registry, parser, prefix map, and cost matching all exist.
- **AutoEffectToken fully refactored** (commit 460fe66). Delegates to `ParseSentence`, uses `Match` API, has debug logging. **No legacy API calls remain.**
- **Integration phase (AD-6) is complete**. `AutoEffectToken`, `ActEffectToken`, and `ContEffectToken` all delegate to `ParseSentence`/`Parse`. ActEffectToken duplication bug fixed. ContEffectToken uses `AggregateToString`.
- **Token regex fixes committed** (f5912d8): `GiveEncoreToOpponentCharactersToken` regex, `AssistPowerBoostToken` trait/`×`, `AssistContEffectToken` merged into `ContEffectToken`, punctuation handling for nested abilities.
- **Condition prefix stripping complete** — all condition tokens now defer to `Type` for prefix generation via `AggregateToString`.
- **Registration order fixes committed** — `NoFacingCharacterOrReversedConditionToken` before `FacingCharacterColorConditionToken`, `PutCardFromHandAndThisToBottomToken` before `PutCardFromHandToWaitingRoomToken`.
- **13 new tokens added** across 3 commits.
- **Cross-check with Comprehensive Fix Plan**: 16 items from the comprehensive plan are now resolved by RC-AD architectural changes. 8 items need re-analysis due to condition prefix stripping and `AggregateToString` changes. See "Cross-Check with Comprehensive Fix Plan" section above.
- **Priority next steps**:
  1. Fix `TryFindFirstMatch` calls in `GainFollowingAbilityToken.TryMatchAny` with `Match` API (~2 failures).
  2. Fix CXCOMBO condition parsing for NIK/S117-087 (Japanese leaking).
  3. Address remaining output format mismatches (trailing periods, spacing) — many may be fixed by condition prefix stripping.
  4. Add more condition/ability tokens for uncovered patterns (RC-M through RC-Z, out of RC-AD scope).
- **Do NOT create new tokens for RC-AD**. The tokens already exist. The problem is either the parsing pipeline or that the regex of existing tokens do not cover it due to missing prefix capture.
- **CSV updates should wait** until RC-AD is fully implemented. Output format may change.
- **Phase ordering matters**: AD-0 → AD-0.5 → AD-1/AD-2 (parallel) → AD-3 → AD-5 → AD-6. AD-4 is handled by AD-0.5.
- **The `し、` particle** is the most common continuation connector. Already in `SkippablePrefixes`.
- **TokenLog**: Debug-only, populated with prefixed token names (e.g. `"Cost:CostRestToken"`, `"Abil:SearchDeckToken"`). Not used for serialization or business logic.
- **Debug logging standard**: All effect tokens and parser classes should have `private static readonly ILogger Log = Serilog.Log.ForContext<ClassName>();` at the top. Use `Log.Debug(...)` for parsing diagnostics. See AGENTS.md Logging section.
