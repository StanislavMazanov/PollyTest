using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PollyHttpClientTest.HttpHandlers {
   public class NoPollyRetryHandler: DelegatingHandler {
       protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
       {
           HttpResponseMessage result = null;
           for (int i = 0; i < 6; i++)
           {
               result = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
               if (result.IsSuccessStatusCode) break;
               Console.WriteLine($"Retrying: {i + 1}");
           }

           return result;
       }
    }
}
