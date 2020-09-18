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
using PollyHttpClientTest.HttpHandlers;

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
            var noPollyRetryHandler = new NoPollyRetryHandler();

            //order of any additional handlers here is important.
            var httpClient = HttpClientFactory.Create(requestHandler, loggingHandler, timeoutHandler, noPollyRetryHandler);
            HttpResponseMessage result = httpClient.GetAsync("https://appulate.com/admin22").GetAwaiter().GetResult();
        }
    }

   
}