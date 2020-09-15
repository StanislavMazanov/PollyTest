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


            var maxRetryAttempts = 3;
            var pauseBetweenFailures = TimeSpan.FromSeconds(2);

            var retryPolicy = Policy
                .Handle<ArgumentException>()
                .WaitAndRetryAsync(maxRetryAttempts, i => pauseBetweenFailures);

            var retryAndExitPolicy = Policy
                .Handle<ArgumentException>()
                .WaitAndRetryAsync(maxRetryAttempts, i => pauseBetweenFailures);

           var t= await retryAndExitPolicy.ExecuteAndCaptureAsync(GetExeption);
            await retryPolicy.ExecuteAsync(GetExeption);



        }

        private  void HttpConnect()
        {


          //  var result = GetAsync("https://example.com/api/products/1").GetAwaiter().GetResult();

            //Console.WriteLine("попытка");
            //var response = httpClient
            //    .GetAsync("https://example.com/api/products/1").GetAwaiter().GetResult();

            HttpMessageHandler httpMessage = new HttpClientHandler();

            var retryEnabledHttpClient = new RetryEnabledHttpClientProxy(httpMessage);



            var response = retryEnabledHttpClient
                .GetAsync("https://github.com/githubappulate").GetAwaiter().GetResult();
            //   .GetAsync("https://example.com/api/products/1").GetAwaiter().GetResult();
        }


        public async Task<HttpResponseMessage> GetAsync(string requestUri) {

            HttpClient client = new HttpClient();
            HttpResponseMessage result = null;
            for (int i = 0; i < 6; i++)
            {
                result = await client.GetAsync(requestUri);
                Thread.Sleep(10000);
                if (result.IsSuccessStatusCode) break;
                Console.WriteLine($"Retrying1: {i + 1}");
            }
            return result;
        }


        private Task GetExeption()
        {
            Console.WriteLine("попытка");
            throw new ArgumentException("test");
        }



    }
}
