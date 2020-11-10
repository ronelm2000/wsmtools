using Flurl.Http;
using Flurl.Http.Configuration;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.Utilities
{
    public class PollyHttpClientFactory : DefaultHttpClientFactory
    {
        public override HttpMessageHandler CreateMessageHandler()
        {
            return new PolicyHandler
            {
                InnerHandler = new TimeoutHandler
                {
                    InnerHandler = base.CreateMessageHandler()
                }
            };  
        }
    }

    public class PolicyHandler : DelegatingHandler
    {
        private ILogger Log = Serilog.Log.ForContext<PolicyHandler>();
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Policies.PolicyStrategy.ExecuteAsync(async () => await base.SendAsync(request, cancellationToken));
        }
    }

    public class TimeoutHandler : DelegatingHandler
    {
        private ILogger Log = Serilog.Log.ForContext<TimeoutHandler>();
        private static TimeSpan Timeout = TimeSpan.FromMilliseconds(1000);

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

        private static AsyncRetryPolicy<HttpResponseMessage> RetryPolicy
        {
            get
            {
                return Policy
                    .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .Or<TimeoutException>()
                    .Or<SocketException>(e => e.SocketErrorCode == SocketError.TimedOut)
                    .Or<SocketException>(e => e.SocketErrorCode == SocketError.OperationAborted)
                    
                    // TODO: Need the ability to set these timeouts on a configuration table.
                    .WaitAndRetryAsync(new[]
                        {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(7),
                        TimeSpan.FromSeconds(14),
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromSeconds(60),
                        TimeSpan.FromSeconds(120),
                        TimeSpan.FromSeconds(240),
                        /*
                        TimeSpan.FromSeconds(480),
                        TimeSpan.FromSeconds(600),
                        TimeSpan.FromSeconds(1200),
                        */
                        },
                        (delegateResult, retryCount, context) =>
                        {
                            var log = Serilog.Log.ForContext<PolicyHandler>();
                            if (delegateResult.Exception != null)
                                Log.Debug($"Exception: \n{delegateResult.Exception.ToString()}");

                            log.Warning($"Retrying after {retryCount} {Translate(delegateResult.Exception)}");
                        });
            }
        }

        private static string Translate(Exception exception)
        {
            if (exception is SocketException se)
                return $"[Socket Exception ({se.ErrorCode} - {se.SocketErrorCode})";
            else if (exception is TimeoutException)
                return "[Timeout]";
            else
                return "";
        }

        public static IAsyncPolicy<HttpResponseMessage> PolicyStrategy => RetryPolicy; //Policy.WrapAsync(RetryPolicy, TimeoutPolicy);
    }
}
