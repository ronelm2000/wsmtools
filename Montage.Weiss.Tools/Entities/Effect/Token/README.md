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
- **Expected Input**: `【永】 あなたの手札が5枚以上なら、このカードのパワーを＋2000。`
- **Structure**: `[CONT] [Labels] During [Conditions], when [Conditions], if [Condition], [Ability].`
- **Labels**: Optional labels like `応援` (Assist), `経験` (Experience) parsed without brackets

#### AutoEffectToken
- **Regex**: `^【自】(?<labels>(?:【[^】]+】)*)\s*(?<mainText>.+)$`
- **Expected Input**: `【自】【ターン1】 ［手札を1枚置く］ あなたのCXが置かれた時、あなたはコストを払ってよい。`
- **Structure**: `[AUTO] [Labels] [CXCOMBO][1/TURN] [<costs>] <During conditions>, <when conditions>, <if conditions>, you may pay the cost. If you do, <actions>.`
- **Labels**: Multiple bracketed labels like `【ターン1】`, `【CXCOMBO】`
- **Costs**: Optional cost in `［...］` format
- **Note**: Do not include brackets if there are no costs. Do not include ", you may pay the cost. If you do," if there are no costs.

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
- **Regex**: `^このカードのパワー＋(\d+)(?:\.|,|、|。)?`
- **Expected Input**: `このカードのパワー＋2000。`
- **Captures**: Power value (e.g., `2000`)
- **Output**: `this card gets +2000 power`

#### SoulBoostToken
- **Regex**: `^このカードのソウルを＋(\d+)(?:\.|,|、|。)?`
- **Expected Input**: `このカードのソウルを＋2。`
- **Captures**: Soul value (e.g., `2`)
- **Output**: `this card gets +2 soul`

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
- **Regex**: `^この能力は1ターンにつき1回まで発動する(?:\.|,|、|。)?`
- **Expected Input**: `この能力は1ターンにつき1回まで発動する。`
- **Output**: `This ability can only be used once per turn`

#### SearchDeckToken
- **Regex**: `^あなたは自分の山札(?:を上から(.+?)枚まで見て、その中から|見て)(《(.+?)》のキャラ|(.+?)を)?(.+?)枚まで選んで相手に見せ、(?:.+?)(?:、.+?)*(?:\.|,|、|。)?`
- **Expected Input**: `あなたは自分の山札を上から4枚まで見て、その中から《風》のキャラを1枚まで選んで相手に見せ、手札に加える。`
- **Captures**: Look count, trait, pick count, additional actions
- **Output**: `search your deck for up to <count> <<trait>> character, reveal it to your opponent`

### 3. Condition Tokens (`CardTextToken<List<CardEffectCondition>>`)

Condition tokens parse conditional clauses. They must be atomic (not compound conditions).

#### HandSizeConditionToken
- **Regex**: `^あなたの手札が(\d+)枚以上なら、`
- **Expected Input**: `あなたの手札が5枚以上なら、`
- **Captures**: Hand count threshold
- **Output**: `If you have 5 or more cards in your hand`
- **Type**: `ConditionType.If`

#### ReverseConditionToken
- **Regex**: `^このカードが【リバース】した時`
- **Expected Input**: `このカードが【リバース】した時`
- **Output**: `When this card becomes [REVERSE]`
- **Type**: `ConditionType.When`

#### DamageCanceledConditionToken
- **Regex**: `^このカードの与えたダメージがキャンセルされた時`
- **Expected Input**: `このカードの与えたダメージがキャンセルされた時`
- **Output**: `When this card's damage is canceled`
- **Type**: `ConditionType.When`

#### CxPlacedConditionToken
- **Regex**: `^あなたのCXがCX置場に置かれた時`
- **Expected Input**: `あなたのCXがCX置場に置かれた時`
- **Output**: `When your CX is placed in the CX area`
- **Type**: `ConditionType.When`

#### ExperienceConditionToken
- **Regex**: `^経験\s*あなたのレベル置場に、「(?<c1>.+?)」と「(?<c2>.+?)」があるなら`
- **Expected Input**: `経験 あなたのレベル置場に、「カードA」と「カードB」があるなら`
- **Captures**: Two card names for experience check
- **Output**: `Experience: If you have "<c1>" and "<c2>" in your level area`
- **Type**: `ConditionType.If`

#### TurnAndTraitCharacterCountConditionToken
- **Regex**: `^あなたのターン中、他のあなたの《(.+?)》のキャラが(\d+)枚以上なら`
- **Expected Input**: `あなたのターン中、他のあなたの《風》のキャラが2枚以上なら`
- **Captures**: Trait and character count
- **Output**: `During your turn, if you have 2 or more other <<風>> characters`
- **Type**: `ConditionType.During` + `ConditionType.If`

#### FacingCharacterColorConditionToken
- **Regex**: `^このカードの正面のキャラが(?<color>.+?)なら`
- **Expected Input**: `このカードの正面のキャラが赤なら`
- **Captures**: Color requirement
- **Output**: `If the character facing this card is <color>`
- **Type**: `ConditionType.If`

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
3. **Non-atomic conditions**: Condition tokens should not capture conjunctions like "and" or "or"
4. **Greedy matching**: Use `.+?` (lazy) instead of `.+` (greedy) when appropriate
5. **Character width**: Handle both `X` (half-width) and `Ｘ` (full-width)
6. **Whitespace variations**: Use `\s*` for optional whitespace

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
