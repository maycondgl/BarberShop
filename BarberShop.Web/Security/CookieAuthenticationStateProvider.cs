using BarberShop.Core.Models.Account;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;

namespace BarberShop.Web.Security
{
    public class CookieAuthenticationStateProvider(IHttpClientFactory clientFactory) : AuthenticationStateProvider, ICookieAuthenticationStateProvider
    {
        private bool _isAuthenticated = false;
        private readonly HttpClient _client = clientFactory.CreateClient(Configuration.HttpClientName);
        public async Task<bool> CheckAuthenticatedAsync()
        {
            await GetAuthenticationStateAsync();
            return _isAuthenticated;
        }
        public void NotifyAuthenticationStateChanged()
          => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            _isAuthenticated = false;

            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());

            MeResponse? me;

            try
            {
                me = await _client.GetFromJsonAsync<MeResponse>("v1/identity/me");
            }
            catch
            {
                return new AuthenticationState(anonymous);
            }

            if (me is null || !me.IsAuthenticated)
                return new AuthenticationState(anonymous);

            var claims = me.Claims
                .Select(x => new Claim(x.Type, x.Value))
                .ToList();

            var identity = new ClaimsIdentity(claims, "Cookies");
            var user = new ClaimsPrincipal(identity);

            _isAuthenticated = true;
            return new AuthenticationState(user);
        }
    }
}
