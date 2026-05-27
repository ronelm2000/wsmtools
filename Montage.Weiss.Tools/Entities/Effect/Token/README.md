# CardTextToken Implementation Guide

## Overview

`CardTextToken<E>` is the base class for all Japanese card text parsing tokens in the Weiss Schwarz translation system. Each token defines a regex pattern that matches specific Japanese text clauses and a translation method that converts the matched text into structured English representations.

## Base Class

```csharp
public abstract class CardTextToken<E>
{
    public abstract Regex Matcher { get; }
    public abstract E Translate(ITokenRegistry registry, ReadOnlyMemory<char> match);
}
```

## Token Categories

There are four categories of tokens, distinguished by their generic type parameter:

| Category | Generic Type | Location | Purpose |
|----------|--------------|----------|---------|
| **Effect Tokens** | `CardTextToken<CardEffect>` | `Token/` (root) | Parse top-level effect types (【永】, 【自】, 【起】, etc.) |
| **Ability Tokens** | `CardTextToken<List<CardEffectAbility>>` | `Token/Ability/` | Parse individual ability clauses within effects |
| **Condition Tokens** | `CardTextToken<List<CardEffectCondition>>` | `Token/Condition/` | Parse conditional clauses (when, if, during) |
| **Reminder Text Tokens** | `CardTextToken<string>` | `Token/ReminderText/` | Parse parenthetical reminder text |

## And Conjunction Guidelines

Token classes should not output `and` directly in the text unless multiple `and` conjunctions are involved in the expected output. Instead, tokens should output a list of atomic `CardEffectAbility` or `CardEffectCondition`, and it should be the job of the parent Token to handle joining them with appropriate conjunctions.

### Rationale

- Atomic tokens handle single clauses without conjunctions
- Parent tokens compose multiple atomic results with proper joining
- Avoids redundancy and maintains consistent conjunction placement
- Makes it easier to restructure output formatting in the future

### Implementation Pattern

**Ability Token (atomic):**
```csharp
// Returns single atomic ability (no "and" in output)
return [ new CardEffectAbility { 
    AbilityText = "this card gets +2000 power"
} ];
```

**Condition Token (atomic):**
```csharp
// Returns single atomic condition (no "and" in output)
return [ new CardEffectCondition {
    ConditionText = "you have 5 or more cards in your hand",
    Type = ConditionType.If
} ];
```

**Parent Token (joins with conjunctions):**
```csharp
// Effect token that combines multiple conditions/abilities
// Parent token handles joining with "and" based on ConditionType
public CardEffect Translate(ITokenRegistry registry, ReadOnlyMemory<char> span) {
    var conditions = new List<CardEffectCondition>();
    var abilities = new List<CardEffectAbility>();
    
    // Parse individual conditions/abilities into lists
    // Atomic tokens return single items; parent token joins them
    
    // Parent token handles joining with "and" as needed via:
    // - CardEffectConditionExtensions.AggregateToString()
    // - AutoEffectToken.JoinAbilityPartsFromSentences()
    
    return new CardEffect {
        Conditions = conditions,
        Abilities = abilities
    };
}
```

## Out-Of-Scope Guidelines

### Names and Traits

- **Names and Traits are NOT translated** at this time
- When a CSV has a translated English name, **change the CSV entry to a JP name**
- Names should be preserved in their original Japanese form in the output

### MatchNameFragment

All tokens that extract a name (`「」`) or trait (`《》`) from card text MUST pass the captured value through `registry.MatchNameFragment(value)` instead of using the raw string directly. This is the centralized stub for future name/trait matching or normalization. Currently returns the input unchanged (identity function).

```csharp
// Correct: use registry.MatchNameFragment for names/traits
var trait = registry.MatchNameFragment(match.Groups[1].Value);
var name = registry.MatchNameFragment(match.Groups["name"].Value);

// Wrong: raw string bypasses the registry
var trait = match.Groups[1].Value;  // DON'T do this
var name = match.Groups["name"].Value;  // DON'T do this
```

### Example

```csharp
// Correct behavior: JP name in English quotes
this card's name is "カード名"
```


## Regex Conventions

### Mandatory Rules

1. **All regex patterns MUST start with `^`** - enforced by `Registry_RegexMustStartWithAnchor` test
2. **Ability tokens MUST end with `(?:\.|,|、|。)?`** - enforced by `Registry_AbilitiesMustCaptureEndingPunctuations` test
3. **Condition tokens MUST be atomic** - they should not capture conjunctions or multiple conditions (enforced by `Registry_ConditionsMustBeAtomic` test)

### Regex Scope Expansion

When modifying token regex patterns, you should **expand the scope** of the regex to handle more variations, not restrict it. This means:

- Add alternative patterns using `(?:pattern1|pattern2)` for optional variations
- Use `\s*` for optional whitespace variations
- Handle both full-width and half-width characters (e.g., `X` and `Ｘ`)
- Add support for additional punctuation variants

### Common Patterns

| Pattern | Purpose | Example |
|---------|---------|---------|
| `(?:\.|,|、|。)?` | Optional ending punctuation (ability tokens) | `^このカードのパワー＋(\d+)(?:\.|,|、|。)?` |
| `\s*` | Optional whitespace | `^[XＸ]\s*は\s*` |
| `(?:pattern1|pattern2)` | Alternative patterns | `^(あなたの|自分の)?` |
| `(?<name>...)` | Named capture groups | `^(?<mainText>.+)$` |
| `(\d+)` | Numeric capture | `^(\d+)枚以上なら、` |

## Expected Clauses by Token Category

### 1. Effect Tokens (`CardTextToken<CardEffect>`)

Effect tokens match the top-level effect type indicator and delegate parsing of conditions and abilities to the appropriate registries.

#### ContEffectToken
- **Regex**: `^【永】\s*(?<mainText>.+)$`
- **Expected Input**: `【永】 あなたの手札が 5 枚以上なら、このカードのパワーを＋2000。`
- **Structure**: `[CONT] [Labels] During [Conditions], when [Conditions], if [Condition], [Ability].`
- **Labels**: Optional labels like `応援` (Assist), `経験` (Experience) parsed without brackets
- **Joining Logic**: Parent token joins multiple conditions using `CardEffectConditionExtensions.AggregateToString()` and multiple abilities using `AutoEffectToken.JoinAbilityPartsFromSentences()` (which handles "and" for serial comma: A, B, and C)

#### AutoEffectToken
- **Regex**: `^【自】(?<labels>(?:【[^】]+】)*)\s*(?<mainText>.+)$`
- **Expected Input**: `【自】【ターン1】 ［手札を 1 枚置く］ あなたの CX が置かれた時、あなたはコストを払ってよい。`
- **Structure**: `[AUTO] [Labels] [CXCOMBO][1/TURN] [<costs>] <During conditions>, <when conditions>, <if conditions>, you may pay the cost. If you do, <actions>.`
- **Labels**: Multiple bracketed labels like `【ターン1】`, `【CXCOMBO】`
- **Costs**: Optional cost in `［...］` format
- **Note**: Do not include brackets if there are no costs. Do not include ", you may pay the cost. If you do," if there are no costs.
- **Joining Logic**: Parent token joins multiple abilities using `AutoEffectToken.JoinAbilityPartsFromSentences()` (handles "and" for serial comma and "If you do" connectors)

#### ActEffectToken
- **Regex**: `^【起】(?<labels>(?:【[^】]+】)*)\s*(?<mainText>.+)$`
- **Expected Input**: `【起】【ターン1］ ［このカードをレストする］ 相手は自分の手札から1枚選ぶ。`
- **Structure**: Similar to AutoEffectToken but for activated abilities

#### CounterEffectToken
- **Expected Input**: `【反】［手札の《風》のキャラを1枚捨てる］ 選択したキャラのパワーを＋2500。`
- **Structure**: Counter effects with optional costs

#### EventEffectToken
- **Regex**: `^.+$`
- **Expected Input**: Any event card text
- **Note**: Catch-all pattern for event cards

#### BrainstormEffectToken
- **Expected Input**: `集中 あなたは自分の山札の上から3枚をめくり、控え室に置く。`
- **Structure**: Brainstorm keyword effects

#### AssistContEffectToken
- **Expected Input**: `【永】 応援 このカードの前のあなたのキャラすべてに、パワーを＋Ｘ。`
- **Structure**: Continuous assist effects

### 2. Ability Tokens (`CardTextToken<List<CardEffectAbility>>`)

Ability tokens parse individual ability clauses. They must capture ending punctuation.

#### SimplePowerBoostToken
- **Regex**: `^このカードのパワー(?:を)?[＋\+]([XＸ\d]+)(?:\.|,|、|。)?`
- **Expected Input**: `このカードのパワー＋2000。`
- **Captures**: Power value (e.g., `2000`)
- **Output**: `this card gets +2000 power`
- **Returns**: `List<CardEffectAbility>` with single atomic ability

#### SoulBoostToken
- **Regex**: `^このカードのソウルを＋(\d+)(?:\.|,|、|。)?`
- **Expected Input**: `このカードのソウルを＋2。`
- **Captures**: Soul value (e.g., `2`)
- **Output**: `this card gets +2 soul`
- **Returns**: `List<CardEffectAbility>` with single atomic ability

#### StockCostToken
- **Regex**: `^\((\d+)\)(?:\.|,|、|。)?`
- **Expected Input**: `(2)`
- **Captures**: Cost value (e.g., `2`)
- **Output**: `(2)`

#### BrainstormToken
- **Regex**: `^集中\s+(?<rest>.+)(?:\.|,|、|。)?`
- **Expected Input**: `集中 あなたは自分の山札の上から3枚をめくり、控え室に置く。`
- **Captures**: Remaining text after `集中` keyword
- **Note**: Parses nested abilities from the remaining text

#### DealDamageToken
- **Regex**: `^相手に[XＸ]\s*ダメージを与える(?:。[XＸ]\s*はそのカードのレベル＋1\s*に等しい(?:\.|,|、|。)?)?(?:\.|,|、|。)?`
- **Expected Input**: `相手にＸダメージを与える。Ｘはそのカードのレベル＋1に等しい。`
- **Captures**: Variable damage with X definition
- **Output**: `deal X damage to your opponent. X is equal to that sent card's level +1`

#### TraitGainToken
- **Regex**: `^このカードは《(.+?)》を得る(?:\.|,|、|。)?`
- **Expected Input**: `このカードは《風》を得る。`
- **Captures**: Trait name (e.g., `風`)
- **Output**: `this card gets the <<風>> trait`

#### XEqualsToken
- **Regex**: `^[XＸ]\s*は\s*(?<description>.+?)\s*に等しい(?:\.|,|、|。)?`
- **Expected Input**: `Ｘは公開されたカードのレベルに等しい。`
- **Captures**: X definition description
- **Output**: `X is equal to <description>`

#### AssistPowerBoostToken
- **Regex**: `^このカードの前のあなたのキャラすべてに、パワーを＋(?:X|\d+)(?:。X はそのキャラのレベル×(\d+) に等しい)?(?:。)?`
- **Expected Input**: `このカードの前のあなたのキャラすべてに、パワーを＋Ｘ。Ｘはそのキャラのレベル×500に等しい。`
- **Captures**: Power boost value or X formula
- **Output**: `All of your characters in front of this card get +X power. X is equal to that character's level x500`

#### ChooseCharacterAndBoostToken
- **Regex**: `^(?:あなたの|自分の)?キャラを(\d+)枚選び、そのターン中、パワーを＋(\d+)(?:\.|,|、|。)?`
- **Expected Input**: `あなたのキャラを1枚選び、そのターン中、パワーを＋3000。`
- **Captures**: Character count and power boost value
- **Output**: `choose <count> of your characters, they get +<power> power this turn`

#### TurnOnceAbilityToken
- **Regex**: `^この能力は 1 ターンにつき 1 回まで発動する(?:\.|,|、|。)?`
- **Expected Input**: `この能力は 1 ターンにつき 1 回まで発動する。`
- **Output**: `This ability can only be used once per turn`
- **Returns**: `List<CardEffectAbility>` with single atomic ability

#### SearchDeckToken
- **Regex**: `^あなたは自分の山札(?:を上から(.+?)枚まで見て、その中から|見て)(《(.+?)》のキャラ|(.+?)を)?(.+?)枚まで選んで相手に見せ、(?:.+?)(?:、.+?)*(?:\.|,|、|。)?`
- **Expected Input**: `あなたは自分の山札を上から4枚まで見て、その中から《風》のキャラを1枚まで選んで相手に見せ、手札に加える。`
- **Captures**: Look count, trait, pick count, additional actions
- **Output**: `search your deck for up to <count> <<trait>> character, reveal it to your opponent`

### 3. Condition Tokens (`CardTextToken<List<CardEffectCondition>>`)

Condition tokens parse conditional clauses. They must be atomic (not compound conditions).

#### HandSizeConditionToken
- **Regex**: `^あなたの手札が(\d+)枚以上なら、`
- **Expected Input**: `あなたの手札が 5 枚以上なら、`
- **Captures**: Hand count threshold
- **Output**: `If you have 5 or more cards in your hand`
- **Type**: `ConditionType.If`
- **Returns**: `List<CardEffectCondition>` with single atomic condition (no "and" in text)

#### ExperienceConditionToken
- **Regex**: `^経験\s*あなたのレベル置場に、「(?<c1>.+?)」と「(?<c2>.+?)」があるなら`
- **Expected Input**: `経験 あなたのレベル置場に、「カード A」と「カード B」があるなら`
- **Captures**: Two card names for experience check
- **Output**: `"カード A" and "カード B" are in your level`
- **Type**: `ConditionType.If`
- **Returns**: `List<CardEffectCondition>` with single atomic condition (contains "and" because Japanese uses `と` for the condition itself)

#### ReverseConditionToken
- **Regex**: `^このカードが【リバース】した時`
- **Expected Input**: `このカードが【リバース】した時`
- **Output**: `When this card becomes [REVERSE]`
- **Type**: `ConditionType.When`
- **Returns**: `List<CardEffectCondition>` with single atomic condition

#### DamageCanceledConditionToken
- **Regex**: `^このカードの与えたダメージがキャンセルされた時`
- **Expected Input**: `このカードの与えたダメージがキャンセルされた時`
- **Output**: `When this card's damage is canceled`
- **Type**: `ConditionType.When`
- **Returns**: `List<CardEffectCondition>` with single atomic condition

#### CxPlacedConditionToken
- **Regex**: `^あなたの CX が CX 置場に置かれた時`
- **Expected Input**: `あなたの CX が CX 置場に置かれた時`
- **Output**: `When your CX is placed in the CX area`
- **Type**: `ConditionType.When`
- **Returns**: `List<CardEffectCondition>` with single atomic condition

#### TurnAndTraitCharacterCountConditionToken
- **Regex**: `^あなたのターン中、他のあなたの《(.+?)》のキャラが (\d+) 枚以上なら`
- **Expected Input**: `あなたのターン中、他のあなたの《風》のキャラが 2 枚以上なら`
- **Captures**: Trait and character count
- **Output**: `During your turn, if you have 2 or more other <<風>> characters`
- **Type**: `ConditionType.During` + `ConditionType.If`
- **Returns**: `List<CardEffectCondition>` with single atomic condition

#### FacingCharacterColorConditionToken
- **Regex**: `^このカードの正面のキャラが (?<color>.+?) なら`
- **Expected Input**: `このカードの正面のキャラが赤なら`
- **Captures**: Color requirement
- **Output**: `If the character facing this card is <color>`
- **Type**: `ConditionType.If`
- **Returns**: `List<CardEffectCondition>` with single atomic condition

#### TurnAndTraitCharacterCountConditionToken
- **Regex**: `^あなたのターン中、他のあなたの《(.+?)》のキャラが(\d+)枚以上なら`
- **Expected Input**: `あなたのターン中、他のあなたの《風》のキャラが 2 枚以上なら`
- **Captures**: Trait and character count
- **Output**: `During your turn, if you have 2 or more other <<風>> characters`
- **Type**: `ConditionType.During` + `ConditionType.If`
- **Returns**: `List<CardEffectCondition>` with single atomic condition

#### FacingCharacterColorConditionToken
- **Regex**: `^このカードの正面のキャラが (?<color>.+?) なら`
- **Expected Input**: `このカードの正面のキャラが赤なら`
- **Captures**: Color requirement
- **Output**: `If the character facing this card is <color>`
- **Type**: `ConditionType.If`
- **Returns**: `List<CardEffectCondition>` with single atomic condition

### 4. Reminder Text Tokens (`CardTextToken<string>`)

Reminder text tokens parse parenthetical clarifications.

#### CxLevelZeroToken
- **Regex**: `^CXのレベルは0として扱う`
- **Expected Input**: `CXのレベルは0として扱う`
- **Output**: `CX are regarded as level 0`

#### ReturnToOriginalPositionToken
- **Regex**: `^公開したカードは元に戻す`
- **Expected Input**: `公開したカードは元に戻す`
- **Output**: `Put it on its original place`

#### ReturnToOriginalPositionOtherwiseToken
- **Regex**: `^そうでないなら元に戻す`
- **Expected Input**: `そうでないなら元に戻す`
- **Output**: `Otherwise, put it back`

#### DamageMayBeCanceledToken
- **Regex**: `^ダメージキャンセルは発生する`
- **Expected Input**: `ダメージキャンセルは発生する`
- **Output**: `Damage may be canceled`

#### BackupCounterReminderToken
- **Expected Input**: Counter reminder text for Backup cards
- **Output**: English reminder text

#### EncoreReminderToken
- **Expected Input**: Encore reminder text
- **Output**: English encore reminder

#### EncoreReminderPart1Token
- **Expected Input**: First part of encore reminder
- **Output**: English part 1

#### EncoreReminderPart2Token
- **Expected Input**: Second part of encore reminder
- **Output**: English part 2

## Implementation Guidelines

### When Adding a New Token

1. **Choose the correct category** based on what the token parses:
   - Effect type indicators → Effect Token
   - Ability clauses → Ability Token
   - Conditional clauses → Condition Token
   - Reminder text → Reminder Text Token

2. **Follow the naming convention**:
   - `<Description>Token.cs`
   - Class name: `<Description>Token`
   - Namespace: `Montage.Weiss.Tools.Entities.Effect.Token.<Category>`

3. **Add XML documentation to the class file** (required for all new/modified tokens):
   - Document the expected Japanese input format
   - Document the regex pattern and what it captures
   - Document the English output format
   - For condition tokens, document the `ConditionType`
   - Include scope expansion notes for future variations
   - Follow the pattern established in existing tokens (e.g., `SimplePowerBoostToken.cs`)

4. **Add `SampleMatches` for audit unit tests**:
   - Provide 1 or more example Japanese inputs that should match this token
   - Include all relevant variations needed for full code coverage
   - This helps ensure the regex is correctly scoped and future changes don't break expected matches

4. **Write the regex**:
   - Start with `^`
   - Use named capture groups for clarity
   - End ability tokens with `(?:\.|,|、|。)?`
   - Handle character width variations (full-width/half-width)

5. **Implement Translate**:
   - Extract captured groups
   - Return structured English representation
   - For ability/condition tokens, return lists
   - For effect tokens, return a single `CardEffect`

6. **Register the token** in `WeissSchwarzCardTranslatorService`:
   - Effect tokens → `_effectRegistry.Register()`
   - Ability tokens → `_effectListRegistry.Register()`
   - Condition tokens → `_conditionListRegistry.Register()`
   - Reminder text tokens → `_reminderTextRegistry.Register()`

7. **Add tests**:
   - Verify regex matches expected inputs
   - Verify translation output matches expected English
   - Add to CSV cross-check if applicable

### When Modifying an Existing Token's Output

Changing a token's translated output (e.g., fixing wording, adding atomic decomposition, changing a character) requires a **cross-referencing audit** to keep all test data consistent. Shuffling passing/failing unit tests without updating their expected values defeats the purpose of the test suite.

#### Audit checklist

For every modified token, run these searches **before** and **after** the change:

1. **CSV test data** — grep **all** CSV files under `Montage.Weiss.Tools.Test/Resources/Translations/` for the old and new output strings. New expansion packs are added as new CSV files in this directory, so a blanket `*.csv` glob catches everything.

   ```
   grep -i "old text" Montage.Weiss.Tools.Test/Resources/Translations/*.csv
   grep -i "new text" Montage.Weiss.Tools.Test/Resources/Translations/*.csv
   ```

   The CSVs are the authoritative source of truth; token output should match CSV expectations, not the reverse, unless the CSV contains an unambiguous error.

2. **Unit test assertions** — grep `TranslatorServiceTests.cs` for the old expected string. Update any assertion that hard-codes the old output.

   ```
   grep -n "old text" Montage.Weiss.Tools.Test/TranslatorServiceTests.cs
   ```

3. **Sibling tokens** — search for tokens whose regex overlaps with the modified token. If two tokens match the same Japanese input, they must produce consistent English wording for equivalent concepts.

   - Check `Condition/` for similar regex patterns (e.g., multiple CX-existence tokens should all use `"there is a CX"` or all use `"a CX is"`, not a mix).
   - The first-registered token wins at match time; any token registered later with a subset of the same pattern is dead code and should be removed.

4. **Glyph consistency** — when the change involves a specific character (multiplication sign `×` vs ASCII `x`, full-width `Ｘ` vs half-width `X`, etc.), verify that the character in the token code matches the character in all CSV files and unit tests.

   Use a hex dump to confirm:
   ```python
   python3 -c "
   with open('file.csv', 'rb') as f:
       content = f.read()
   idx = content.find(b'search term')
   print(content[idx:idx+20].hex())
   "
   ```

5. **Full test pass** — run both the CSV cross-check and the individual unit tests after the change:

   ```bash
   dotnet test --filter "FullyQualifiedName~Translate_CSV_CrossCheckAll"
   dotnet test --filter "TestCategory=CI"
   ```

   The CSV count should not regress unless the CSV data was the source of the inconsistency and was updated intentionally.

#### Example: modifying the `×` character

| Step | Command | What to check |
|------|---------|---------------|
| 1 | `grep "level x500\|level ×500" *.csv` | Which CSVs use ASCII `x` vs `×`? |
| 2 | `grep "level x500\|level ×500" *Tests.cs` | Do unit tests use the same character? |
| 3 | Check sibling tokens | Do all Assist tokens use the same glyph? |
| 4 | Hex-verify | `0x78` = ASCII `x`, `0xD7` = `×` |
| 5 | Full test pass | CSV count must not regress |

#### Example: changing `CxWithTriggerIconInCxAreaConditionToken` wording

The no‑icon branch of `CxWithTriggerIconInCxAreaConditionToken` was changed from `"a CX is in your CX area"` to `"there is a CX in your CX area"`. The audit revealed:

1. CSV `expansion_494_effects.csv:173` already used `"there is a CX"` → no CSV change needed.
2. New token `CxAreaHasCxConditionToken` had `"a CX is"` — inconsistent → updated to `"there is a CX"`.
3. Unit tests did not reference this string → no change needed.
4. New token `CxExistsConditionToken` was dead code (its input is a subset of `CxWithTriggerIconInCxAreaConditionToken`'s pattern) → removed.

### When Modifying an Existing Token (Code Changes)

Changing a token's code — regex pattern, capture groups, or translation logic — requires updating related artifacts and verifying consistency across the codebase.

#### Checklist

1. **Update `SampleMatches`** — If the regex changed, the `SampleMatches` entries (used by `Registry_SampleMatchesMustMatchRegex` and similar audit tests) must reflect the new pattern. Add new examples for any added alternatives and remove examples that no longer match.

2. **Update XML documentation** — Keep the class-level `<remarks>` in sync with the current regex, captures, and output format. Stale docs mislead future maintainers.

3. **Regression test & usage audit** — Run the full CSV cross-check to catch regressions:
   ```bash
   dotnet test --filter "Translate_CSV_CrossCheckAll"
   ```
   Then find all references to the modified token to audit callers:
   ```bash
   grep -rn "YourTokenName" Montage.Weiss.Tools/ Montage.Weiss.Tools.Test/
   ```
   Check each usage for:
   - Direct instantiation in test assertions or mock registries
   - Registration in `WeissSchwarzCardTranslatorService`
   - Overlapping regex coverage with sibling tokens (dead-code risk)

#### Common pitfalls

- **Dead code from overlapping regex**: A new (or modified) token with a narrower regex that is a strict subset of an earlier-registered token's regex will never match. Remove it.
- **Baked-in `if` / `When` prefixes**: Condition tokens must NOT include `"if"`, `"When"`, `"During"` etc. in their `ConditionText`. These prefixes are prepended by `CardEffectConditionExtensions.AggregateToString` based on `ConditionType`. Baking them in produces double-prefix output like `"If if there is a CX"`.
- **Trailing period**: `JoinAbilityPartsFromSentences` appends a trailing `.` to ability text when absent. If a unit test asserts `AbilityText` without a trailing period, it must be updated to include it.
- **Single-character glyphs**: `×` (U+00D7, multiplication sign), `x` (U+0078, ASCII x), and `Ｘ` (U+FF38, full-width X) look nearly identical in some fonts but are distinct code points. Use byte-level inspection to confirm them.

### Documentation Template for Class Files

Every token class must include XML documentation following this template:

```csharp
/// <summary>
/// Brief description of what this token matches.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>Japanese text example here</c></para>
/// <para><b>Regex:</b> ^your-regex-pattern-here</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: What this group captures (e.g., "Power value")</description></item>
///   <item><description>Named group: What this group captures</description></item>
/// </list>
/// <para><b>Output:</b> <c>English translation example</c></para>
/// <!-- For condition tokens only: -->
/// <para><b>Type:</b> <c>ConditionType.When</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Variation 1 description
/// - Variation 2 description</para>
/// </remarks>
```

**Key points:**
- Use `<c>` tags for code/regex/inline text
- Use `<list type="bullet">` for capture groups
- Include scope expansion notes to guide future modifications
- For condition tokens, document the `ConditionType`
- For effect tokens, document the expected full English format

### Common Pitfalls

1. **Missing `^` anchor**: All tokens must start with `^`
2. **Missing punctuation capture**: Ability tokens must end with `(?:\.|,|、|。)?`
3. **Non-atomic conditions**: Condition tokens should not capture conjunctions like "and" or "or" unless the Japanese uses a conjunction (e.g., `と` in ExperienceConditionToken)
4. **Greedy matching**: Use `.+?` (lazy) instead of `.+` (greedy) when appropriate
5. **Character width**: Handle both `X` (half-width) and `Ｘ` (full-width)
6. **Whitespace variations**: Use `\s*` for optional whitespace
7. **Returning lists**: All tokens must return `List<T>` (even with single items), not single objects
8. **No "and" in atomic tokens**: Atomic ability/condition tokens should not output "and" directly; parent tokens join with conjunctions via `AggregateToString()` and `JoinAbilityPartsFromSentences()`

### Testing

Run the following tests to validate token implementations:

```bash
# Run all token-related tests
dotnet test --filter TranslatorServiceTests

# Run specific validation tests
dotnet test --filter "Registry_RegexMustStartWithAnchor"
dotnet test --filter "Registry_AbilitiesMustCaptureEndingPunctuations"
dotnet test --filter "Registry_ConditionsMustBeAtomic"

# Run CSV cross-check tests
dotnet test --filter "Translate_CSV_CrossCheckAll"
```

## Registry Flow

1. **Effect Token** matches the top-level effect type (【永】, 【自】, etc.)
2. **Condition Tokens** are iteratively matched from the start of remaining text
3. **Ability Tokens** are iteratively matched from the remaining text after conditions
4. **Reminder Text Tokens** are matched from parenthetical text at the end

The `ComponentRegistry<E>` class manages token registration and matching:
- `Register(token)` - adds a token to the registry
- `TryMatchAtStart(input, ...)` - tries to match at the start of input
- `TryFindFirstMatch(input, ...)` - finds the first match anywhere in input
- `GetMatch(input)` - returns a translation function for the input

## Excluded Ability Tokens

The following ability tokens are excluded from the `Registry_AbilitiesMustCaptureEndingPunctuations` test because they perform greedy captures:

- `PowerBoostWithFollowingAbilityToken`
- `GiveMultipleAbilitiesToken`
- `EncoreToken`
- `BackupPrefixToken`
- `IfYouDoToken`
- `ChooseOtherCharacterAndGiveAbilityToken`

These tokens are designed to capture larger text spans and should not be modified to capture ending punctuation without careful consideration of the implications on translation results.
