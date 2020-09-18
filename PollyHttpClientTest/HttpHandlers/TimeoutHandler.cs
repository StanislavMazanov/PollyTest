using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PollyHttpClientTest {
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
                return await base.SendAsync(request, linkedToken.Token);
            }
            catch (OperationCanceledException) when (timeoutToken.IsCancellationRequested)
            {
                Console.WriteLine("Time Out");
                throw new TimeoutException();
            }
        }
    }
}
