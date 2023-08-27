using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;

namespace Montage.Weiss.Tools.Utilities;

public static class TaskExtensions
{
    public static async Task<T> WithRetries<T>(this Task<T> task, int retries)
    {
        Exception temp = new Exception();
        for (int i = 0; i <= retries; i++)
            try
            {
                return await task;
            }
            catch (TaskCanceledException)
            {
                // Rethrow TaskCancelledException as it needs to be cancelled as soon as possible.
                throw;
            }
            catch (Exception e)
            {
                if (i == retries) temp = e;
            }
        throw temp;
    }

    public static async IAsyncEnumerable<R> SelectParallelAsync<T, R>(this IAsyncEnumerable<T> source, Func<T, Task<R>> body, [EnumeratorCancellation] CancellationToken cancellationToken = default!, int maxDegreeOfParallelism = DataflowBlockOptions.Unbounded, TaskScheduler? scheduler = null)
    {
        var options = new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism,
            CancellationToken = cancellationToken
        };
        if (scheduler is not null)
            options.TaskScheduler = scheduler;

        var transformBlock = new TransformBlock<T, Task<R>>(body, options);

        // Queue everything.
        await foreach (var item in source)
            transformBlock.Post(item);
        transformBlock.Complete();

        // Recieve Everything Asynchronously
        await foreach (var task in transformBlock.ReceiveAllAsync(cancellationToken))
            yield return await task;

        // Post Completion in case of cancellation
        await transformBlock.Completion;
    }
}
