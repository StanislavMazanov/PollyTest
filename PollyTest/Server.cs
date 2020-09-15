using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Polly;

namespace PollyTest
{
   public class Server
    {

        public async void Run()
        {
            var httpClient = new HttpClient();
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

        private  void HttpConnect(HttpClient httpClient)
        {
            Console.WriteLine("попытка");
            var response = httpClient
                .GetAsync("http://localhost:54532/Home/BadResult").GetAwaiter().GetResult();
                //  .GetAsync("https://example.com/api/products/1").GetAwaiter().GetResult();
        }


        private Task GetExeption()
        {
            Console.WriteLine("попытка");
            throw new ArgumentException("test");
        }



    }
}
