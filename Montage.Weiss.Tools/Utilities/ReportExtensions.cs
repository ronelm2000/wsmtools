using Montage.Card.API.Entities;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Entities;

namespace Montage.Weiss.Tools.Utilities;

public static class ReportExtensions
{
    public static DeckParserProgressReport ObtainedParseDeckData(this DeckParserProgressReport report, WeissSchwarzDeck deck)
    {
        return report with
        {
            Percentage = 10,
            ReportMessage = new Card.API.Entities.Impls.MultiLanguageString { EN = $"Obtained data for [{deck.Name}]" }
        };
    }

    public static DeckParserProgressReport SuccessfullyParsedDeck(this DeckParserProgressReport report, WeissSchwarzDeck deck)
    {
        return report with
        {
            Percentage = 100,
            ReportMessage = new Card.API.Entities.Impls.MultiLanguageString { EN = $"Successfully Parsed [{deck.Name}]" }
        };
    }

    public static R AsRatio<T,R>(this T report, int ratioBase, float ratioInPercentage) where T : UpdateProgressReport where R : UpdateProgressReport, new()
       => new R()
       {
           ReportMessage = report.ReportMessage,
           Percentage = ratioBase + (int)(report.Percentage * ratioInPercentage)
       };

    /*
    public static IProgress<T> GetAggregator<T,A>(this IProgress<A> aggregateProgressReporter, Func<T,A> aggregateFunction)
        => new AggregateProgress<T,A>(aggregateProgressReporter, aggregateFunction);

    public static IProgress<T> GetSimpleRatioAggregator<T,A>(this IProgress<A> aggregateProgressReporter, int ratioBase, float ratioInPercentage)
        => new AggregateProgress<T, A>(aggregateProgressReporter, r => AsRatio<T,A>(r, ratioBase, ratioInPercentage));
    */

    public static ProgressBuilder<T> From<T>(this IProgress<T> outProgress)
        => new ProgressBuilder<T>(outProgress);

    public class ProgressBuilder<T>
    {
        private IProgress<T> outProgress;

        internal ProgressBuilder(IProgress<T> outProgress)
        {
            this.outProgress = outProgress;
        }

        public IProgress<O> Translate<O>(Func<O,T> func)
        {
            return new AggregateProgress<O,T>(outProgress, func);
        }
    }

    private class AggregateProgress<I,O> : IProgress<I>
    {
        private readonly IProgress<O> _aggregateProgressReporter;
        private readonly Func<I,O> _aggregateFunction;

        internal AggregateProgress(IProgress<O> aggregateProgressReporter, Func<I,O> aggregateFunction)
        {
            _aggregateProgressReporter = aggregateProgressReporter;
            _aggregateFunction = aggregateFunction;
        }

        public void Report(I value)
        {
            _aggregateProgressReporter.Report(_aggregateFunction(value));
        }
    }
}
