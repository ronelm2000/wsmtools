using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Entities.Expressions;
using Montage.Weiss.Tools.Entities.Expressions.Conditions;
using Montage.Weiss.Tools.Entities.Expressions.Effects;
using Montage.Weiss.Tools.Entities.Expressions.Zones;
using Montage.Weiss.Tools.Entities.External.Cockatrice;
using Montage.Weiss.Tools.Test.Commons;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.Internal;

[TestClass]
public class ExpressionTest
{
    [TestMethod("Serial Parser Test")]
    public void TestExpressionTrees()
    {
        Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();

        var options = new JsonSerializerOptions() { TypeInfoResolver = new WeissSchwarzExpressionTypeResolver() };
        var expression = GetSample();
        var expressionStr = JsonSerializer.Serialize(expression, options);        
        var expressionDeserialized = JsonSerializer.Deserialize<WeissSchwarzExpression>(expressionStr, options);
        var expressionDeserializedStr = JsonSerializer.Serialize(expression, options);


        Log.Information("Expression: {@exp}", expression);
        Log.Information($"Result: {expressionStr}");
        Log.Information("Expression After Deserialization: {@exp}", expressionDeserialized);

        Assert.IsTrue(expressionStr == expressionDeserializedStr);
        //Log.Information("Result from Serilog: {@expression}", expression);
    }

    // [CONT] Assist All of your characters in front of this card gets +500 power.
    // [C] ASSIST All your Characters in front of this gain +500 Power. 
    private static WeissSchwarzExpression GetSample()
    {
        return new WeissSchwarzExpression()
        {
            ExpType = ExpressionType.Continuous,
            Labels = new[] { Label.Assist },
            Condition = null,
            Effect = new GainStatEffect
            {
                Stat = StatType.ATK,
                Expression = new ConstantStatExpression { Value = +500 },
                Target = Player.You.Stage().InFrontOf(Target.This)
            }
        };
    }

    // [AUTO] [Put a card from your hand into your waiting room] When this card is placed on stage from your hand, you may pay the cost.
    // If you do, look at up to 4 cards from the top of your deck, search for up to 1 level 1 or higher card from among them, add it into your hand,
    // and put the rest into your waiting room.
    private static WeissSchwarzExpression GetSample2()
    {
        return new WeissSchwarzExpression()
        {
            ExpType = ExpressionType.Automated,
            Labels = Array.Empty<Label>(),
            Condition = Target.This.Is().Placed(Zones.Stage, Zones.Hand),
            Effect = new PayCostEffect
            {
                Cost = new ChooseEffect
                {
                    Target = Zones.Hand,
                    Amount = 1,
                    Then = new SendCardEffect
                    {
                        Target = Zones.WaitingRoom
                    }
                },
                IfYouDo = new LookAtTopEffect
                {
                    Target = Zones.Deck,
                    UpTo = true,
                    Amount = 4,
                    Then = new ChooseEffect
                    {
                        Target = Zones.Resolution with
                        {
                            Condition = new MatchCharacterStatCondition
                            {
                                Stat = StatType.Level,
                                Expression = new ConstantStatExpression { Value = 1 },
                                OrHigher = true
                            }
                        },
                        Then = new SendCardEffect
                        {
                            Target = Zones.Hand
                        }.And( new SendCardEffect
                        {
                            Source = Zones.Resolution,
                            Target = Zones.WaitingRoom
                        })
                    }
                }
            }
        };
    }
}
