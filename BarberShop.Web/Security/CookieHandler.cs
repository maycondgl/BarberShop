using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace BarberShop.Web.Security
{
    public class CookieHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            request.Headers.Add("X-Requested-with", ["XMLHttpRequest"]);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
