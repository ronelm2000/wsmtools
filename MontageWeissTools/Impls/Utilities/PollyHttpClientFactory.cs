using Flurl.Http;
using Flurl.Http.Configuration;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Montage.Weiss.Tools.Entities;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;

namespace Montage.Weiss.Tools.Impls.Utilities;

public class PollyHttpClientFactory : DefaultHttpClientFactory
{
    private ILogger Log = Serilog.Log.ForContext<PollyHttpClientFactory>();
    private Func<CardDatabaseContext> _db;

    public PollyHttpClientFactory(IContainer ioc)
    {
        Log.Debug("Starting...");
        _db = () => ioc.GetInstance<CardDatabaseContext>();
    }

    public override HttpMessageHandler CreateMessageHandler()
    {
        using (var db = _db())
        {
            db.Database.Migrate();
            var maxRetries = db.Settings.Find("http.retries")?.GetValue<int>() ?? 10;
            return new PolicyHandler(maxRetries)
            {
                InnerHandler = new TimeoutHandler
                {
                    InnerHandler = base.CreateMessageHandler(),
                    Timeout = TimeSpan.FromSeconds(db.Settings.Find("http.timeout")?.GetValue<int>() ?? 100)
                }
            };
        }
    }
}

public class PolicyHandler : DelegatingHandler
{
    private ILogger Log = Serilog.Log.ForContext<PolicyHandler>();

    public IAsyncPolicy<HttpResponseMessage> PolicyStrategy { get; set; }

    public PolicyHandler(int maxTimeouts)
    {
        this.PolicyStrategy = Policies.GenerateRetryPolicy(maxTimeouts).WrapAsync(Policies.GenerateFlurlTransientRetryPolicy(maxTimeouts));
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return PolicyStrategy.ExecuteAsync(async () => await base.SendAsync(request, cancellationToken));
    }
}

public class TimeoutHandler : DelegatingHandler
{
    private ILogger Log = Serilog.Log.ForContext<TimeoutHandler>();

    public int MaximumRetries { get; internal set; }
    public TimeSpan Timeout { get; internal set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(Timeout);
        var timeoutToken = cts.Token;

        var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken);

        try
        {
            return await base.SendAsync(request, linkedToken.Token);
        }
        catch (OperationCanceledException) when (timeoutToken.IsCancellationRequested)
        {
            throw new TimeoutException();
        }
    }
}

public static class Policies
{
    private static AsyncTimeoutPolicy<HttpResponseMessage> TimeoutPolicy
    {
        get
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(5, (context, timeSpan, task) =>
            {
                var log = Serilog.Log.ForContext<PolicyHandler>();
                log.Debug($"Timeout delegate fired after {timeSpan.Seconds} seconds");
                return Task.CompletedTask;
            });
        }
    }

    internal static AsyncRetryPolicy<HttpResponseMessage> GenerateRetryPolicy(int maxRetries)
    {   
        return Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<TimeoutException>()
                .Or<SocketException>(e => e.SocketErrorCode == SocketError.TimedOut)
                .Or<SocketException>(e => e.SocketErrorCode == SocketError.OperationAborted)                    
                .WaitAndRetryAsync(
                    Enumerable.Range(0, maxRetries).Select(i => TimeSpan.FromSeconds(Math.Pow(2, i))),
                    (delegateResult, retryCount, context) =>
                    {
                        var log = Serilog.Log.ForContext<PolicyHandler>();
                        if (delegateResult.Exception != null)
                            Log.Debug($"Exception: \n{delegateResult.Exception.ToString()}");

                        log.Warning($"Retrying after {retryCount} {Policies.Translate(delegateResult.Exception!)}");
                    });
    }

    private static string Translate(Exception exception)
    {
        if (exception is SocketException se)
            return $"[Socket Exception ({se.ErrorCode} - {se.SocketErrorCode})";
        else if (exception is TimeoutException)
            return "[Timeout]";
        else if (exception is FlurlHttpException flurlException)
            return $"[{flurlException.StatusCode ?? -1} {flurlException.Message}]";
        else
            return "";
    }

    internal static AsyncRetryPolicy GenerateFlurlTransientRetryPolicy(int maxRetries)
    {
        var retryPolicy = Policy
           .Handle<FlurlHttpException>(IsTransientError)
           .WaitAndRetryAsync(
               Enumerable.Range(0, maxRetries).Select(i => TimeSpan.FromSeconds(Math.Pow(2, i))),
               (delegateResult, retryCount, context) =>
               {
                   var log = Serilog.Log.ForContext<PolicyHandler>();
                   log.Warning($"Retrying after {retryCount} {Policies.Translate(delegateResult)}");
               });

        return retryPolicy;
    }
    private static bool IsTransientError(FlurlHttpException exception)
    {
        int[] httpStatusCodesWorthRetrying =
        {
            (int)HttpStatusCode.RequestTimeout, // 408
            (int)HttpStatusCode.BadGateway, // 502
            (int)HttpStatusCode.ServiceUnavailable, // 503
            (int)HttpStatusCode.GatewayTimeout // 504
        };
        return exception.StatusCode.HasValue && httpStatusCodesWorthRetrying.Contains(exception.StatusCode.Value);
    }
    //public static IAsyncPolicy<HttpResponseMessage> PolicyStrategy => RetryPolicy; //Policy.WrapAsync(RetryPolicy, TimeoutPolicy);
}
