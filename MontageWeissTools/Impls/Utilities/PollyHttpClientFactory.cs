﻿using Flurl.Http;
using Flurl.Http.Configuration;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Montage.Weiss.Tools.Entities;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                var maxRetries = db.Settings.Find("http.retries")?.GetValue<int>() ?? 10;
                return new PolicyHandler(maxRetries)
                {
                    InnerHandler = new TimeoutHandler
                    {
                        InnerHandler = base.CreateMessageHandler()
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
            this.PolicyStrategy = Policies.GenerateRetryPolicy(maxTimeouts);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return PolicyStrategy.ExecuteAsync(async () => await base.SendAsync(request, cancellationToken));
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

        internal static AsyncRetryPolicy<HttpResponseMessage> GenerateRetryPolicy(int maxRetries)
        {   
            return Policy
                    .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .Or<TimeoutException>()
                    .Or<SocketException>(e => e.SocketErrorCode == SocketError.TimedOut)
                    .Or<SocketException>(e => e.SocketErrorCode == SocketError.OperationAborted)
                    
                    // TODO: Need the ability to set these timeouts on a configuration table.
                    .WaitAndRetryAsync(
                        Enumerable.Range(0, maxRetries).Select(i => TimeSpan.FromSeconds(Math.Pow(2, i))),
                        (delegateResult, retryCount, context) =>
                        {
                            var log = Serilog.Log.ForContext<PolicyHandler>();
                            if (delegateResult.Exception != null)
                                Log.Debug($"Exception: \n{delegateResult.Exception.ToString()}");

                            log.Warning($"Retrying after {retryCount} {Policies.Translate(delegateResult.Exception)}");
                        });
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

        //public static IAsyncPolicy<HttpResponseMessage> PolicyStrategy => RetryPolicy; //Policy.WrapAsync(RetryPolicy, TimeoutPolicy);
    }
}
