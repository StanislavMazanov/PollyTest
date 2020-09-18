using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Http;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;

namespace PollyHttpClientTest
{
    public class RetryEnabledHttpClientProxy : IDisposable
    {
        private readonly HttpClient _httpClient;


        ~RetryEnabledHttpClientProxy()
        {
            Dispose(disposing: false);
        }

        private const int MaxRetryAttempts = 20;
        private TimeSpan _pauseBetweenFailures = TimeSpan.FromSeconds(2);
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        /// <summary>
        ///     Please, do not use constructor directly. HttpClientProxy should be used only as SingleInstance services, registered
        ///     in Autofac
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Scope = "Member",
            Justification = "Delegation of ownership")]
        public RetryEnabledHttpClientProxy(HttpMessageHandler httpMessageHandler)
        {
            AsyncRetryPolicy<HttpResponseMessage> retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError() // HttpRequestException, 5XX and 408
                .OrResult(r => r.StatusCode == HttpStatusCode.NotFound)
                .WaitAndRetryAsync(new[] {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10)
                }, (x, i) => x.Result.Dispose());
            var policyHttpMessageHandler = new PolicyHttpMessageHandler(retryPolicy);
            try {
                var timeoutHandler = new TimeoutHandler {InnerHandler = httpMessageHandler};
                policyHttpMessageHandler.InnerHandler = timeoutHandler;
                _httpClient = new HttpClient(policyHttpMessageHandler);
            } catch {
                policyHttpMessageHandler.Dispose();
                throw;
            }
        }

        public async Task<HttpResponseMessage> GetAsync(string requestUri) {
            return await _httpClient.GetAsync(requestUri);
        }

        public async Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken)
        {
            return await _httpClient.GetAsync(requestUri, cancellationToken);
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            return await _httpClient.PostAsync(requestUri, content);
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content,
            CancellationToken cancellationToken)
        {
            return await _httpClient.PostAsync(requestUri, content, cancellationToken);
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage)
        {
            return _httpClient.SendAsync(requestMessage);
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage,
            CancellationToken cancellationToken)
        {
            return await _httpClient.SendAsync(requestMessage, cancellationToken);
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage,
            HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            return await _httpClient.SendAsync(requestMessage, completionOption, cancellationToken);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _httpClient.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}