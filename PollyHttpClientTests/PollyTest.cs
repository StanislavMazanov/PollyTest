using NUnit.Framework;
using PollyHttpClientTest;
using System.Net.Http;
using System.Threading.Tasks;
using PollyHttpClientTest.HttpHandlers;

namespace PollyHttpClientTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task HttpClientNoPollyUse()
        {
            var urlTest = "https://appulate.com/admin22";
            var requestHandler = new HttpClientHandler();
            var loggingHandler = new LoggingHandler();
            var timeoutHandler = new TimeoutHandler();
            var noPollyRetryHandler = new NoPollyRetryHandler();

            //order of any additional handlers here is important.
            var httpClient =
                HttpClientFactory.Create(requestHandler, loggingHandler, timeoutHandler, noPollyRetryHandler);

           var response = await httpClient.GetAsync(urlTest);
        }

        [Test]
        public async Task HttpClientPollyUse()
        {
            var urlTest = "https://appulate.com/admin22";
            HttpMessageHandler httpMessage = new HttpClientHandler();
            var httpClient = new RetryEnabledHttpClientProxy(httpMessage);
            var response = await httpClient.GetAsync(urlTest);
        }
    }
}