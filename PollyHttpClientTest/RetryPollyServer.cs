using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace PollyHttpClientTest
{
    public class RetryPollyServer
    {
        public void Run()
        {
            var requestHandler = new HttpClientHandler();
              var loggingHandler = new LoggingHandler();
            var timeoutHandler = new TimeoutHandler();
            var retryHandler = new RetryHandler();

            //order of any additional handlers here is important.
            var httpClient = HttpClientFactory.Create(requestHandler, loggingHandler, retryHandler);
            // httpClient.Timeout = TimeSpan.FromMilliseconds(20);
          //  HttpResponseMessage result = await httpClient.GetAsync("http://localhost:54532/Home/BadResult");
           HttpResponseMessage result =  httpClient.GetAsync("https://appulate.com/admin22").GetAwaiter().GetResult();
        }
    }
    public static class HttpClientFactory
    {
        public static HttpClient Create(HttpClientHandler requestHandler, LoggingHandler loggingHandler, RetryHandler retryHandler)
        {
            loggingHandler.InnerHandler = requestHandler;
            retryHandler.InnerHandler = loggingHandler;

            return new HttpClient(retryHandler);
        }

        public static HttpClient Create(HttpClientHandler requestHandler, TimeoutHandler timeoutHandler)
        {
            timeoutHandler.InnerHandler = requestHandler;

            return new HttpClient(timeoutHandler);
        }

        public static HttpClient Create(HttpClientHandler requestHandler, RetryHandler retryHandler)
        {
            retryHandler.InnerHandler = requestHandler;

            return new HttpClient(retryHandler);
        }

        public static HttpClient Create(HttpClientHandler requestHandler, RetryHandler retryHandler,
            TimeoutHandler timeoutHandler)
        {
            retryHandler.InnerHandler = requestHandler;
            timeoutHandler.InnerHandler = retryHandler;


            return new HttpClient(retryHandler);
        }
    }

    public class LoggingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken token)
        {
            try
            {
                Console.WriteLine("message for me!");
                return await base.SendAsync(request, token);
            }
            catch (TaskCanceledException ex)
            {
                // ex.CancellationToken.IsCancellationRequested();
                // token.IsCancellationRequested;
                throw ex;
            }
        }
    }

    public class TimeoutHandler : DelegatingHandler
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(10000);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(Timeout);
            var timeoutToken = cts.Token;

            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken);

            try
            {
                return  base.SendAsync(request, linkedToken.Token).Result;
            }
            catch (OperationCanceledException) when (timeoutToken.IsCancellationRequested)
            {
                throw new TimeoutException();
            }
        }
    }

    public class RetryHandler : DelegatingHandler
    {
        private readonly AsyncRetryPolicy<HttpResponseMessage> _policy;


        public RetryHandler()
        {
            var maxRetryAttempts = 10;
            var pauseBetweenFailures = TimeSpan.FromSeconds(5);
            _policy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.NotFound)
                .WaitAndRetryAsync(2, i => pauseBetweenFailures);


            //var retryPolicy = Policy
            //    .HandleResult<HttpResponseMessage>(x => !x.IsSuccessStatusCode)
            //    .WaitAndRetryAsync(maxRetryAttempts, i => pauseBetweenFailures);
        }

        private void OnRetry(DelegateResult<HttpResponseMessage> arg1, int arg2)
        {
            Console.WriteLine("Retrying timed out request");
            // Console.WriteLine(ex);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            try
            {
                return await _policy.ExecuteAsync( async () => await base.SendAsync(request, cancellationToken));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        //private void OnRetry(HttpResponseMessage ex, int retryCount)
        //{
        //    Console.WriteLine("Retrying timed out request");
        //    Console.WriteLine(ex);
        //}
    }
}