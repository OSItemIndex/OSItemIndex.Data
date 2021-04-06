using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OSItemIndex.AggregateService
{
    public class ObserverHttpMessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("User-Agent", Constants.ObserverUserAgent);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
