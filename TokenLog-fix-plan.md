# TokenLog Fix Plan

## Problem

`TokenLog` entries for conditions/abilities from `ParseSentence` log record
class names (`Cond:HandSizeCondition`, `Abil:PowerBoostAbility`) instead
of token class names (`Cond:HandSizeConditionToken`, `Abil:SimplePowerBoostToken`).

Cost tokens work correctly because they log `m.Match.Token` directly.

## Approach

Add `List<string> ConditionTokenNames` and `List<string> AbilityTokenNames`
to `ParsedSentence` record. Populate in `ParseSentence` from `Match.Token`.
Use these lists in effect token classes instead of `c.GetType().Name`.

## Files

| File | Change |
|------|--------|
| `MultiClauseEffectParser.cs` | Add fields to record; populate at 5 match sites + CatchAll |
| `AutoEffectToken.cs` | Replace `GetType().Name` loops with `AddRange` from ParsedSentence |
| `ContEffectToken.cs` | Same |
| `ActEffectToken.cs` | Same |
