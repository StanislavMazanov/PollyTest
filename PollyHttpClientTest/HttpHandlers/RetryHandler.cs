using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;

namespace PollyHttpClientTest.HttpHandlers
{
    public class RetryHandler : DelegatingHandler
    {
        private readonly AsyncRetryPolicy<HttpResponseMessage> _policy;

        public RetryHandler()
        {
            const int maxRetryAttempts = 10;
            var pauseBetweenFailures = TimeSpan.FromSeconds(2);
            _policy = Policy
                .Handle<TimeoutException>()
                .OrResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.NotFound)
                .WaitAndRetryAsync(maxRetryAttempts, i => pauseBetweenFailures, (x, i) => x.Result.Dispose());
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return await _policy.ExecuteAsync(async () => await base.SendAsync(request, cancellationToken));
        }
    }
}