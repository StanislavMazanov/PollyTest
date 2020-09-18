using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using PollyHttpClientTest.HttpHandlers;

namespace PollyHttpClientTest {
    public static class HttpClientFactory
    {
        public static HttpClient Create(HttpClientHandler requestHandler, LoggingHandler loggingHandler,
            TimeoutHandler timeoutHandler, RetryHandler retryHandler)
        {
            retryHandler.InnerHandler = requestHandler;
            timeoutHandler.InnerHandler = retryHandler;
            loggingHandler.InnerHandler = timeoutHandler;


            return new HttpClient(loggingHandler);
        }

        public static HttpClient Create(HttpClientHandler requestHandler, LoggingHandler loggingHandler,
            TimeoutHandler timeoutHandler, NoPollyRetryHandler retryHandler)
        {
            retryHandler.InnerHandler = requestHandler;
            timeoutHandler.InnerHandler = retryHandler;
            loggingHandler.InnerHandler = timeoutHandler;


            return new HttpClient(loggingHandler);
        }
    }
}
