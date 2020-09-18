using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace PollyHttpClientTest
{
    public class Server
    {
        public async void Run()
        {
            HttpConnect();
        }

        private async void HttpConnect()
        {
            HttpMessageHandler httpMessage = new HttpClientHandler();
            var retryEnabledHttpClient = new RetryEnabledHttpClientProxy(httpMessage);
            var response = await retryEnabledHttpClient
                .GetAsync("https://github.com/githubappulate");
        }
    }
}