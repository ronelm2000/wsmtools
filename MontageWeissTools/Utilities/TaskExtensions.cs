using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

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

    public async static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this ParallelQuery<T> parallelQuery, [EnumeratorCancellation] CancellationToken token = default)
    {
        var queue = new ConcurrentQueue<T>();
        var task = Task.Run(() => parallelQuery.WithCancellation(token).ForAll(i => queue.Enqueue(i)), token); ;
        while (!task.IsCompleted)
            if (queue.TryDequeue(out var result) && result is not null)
                yield return result;
        await task;
    }
}
