using Client.Interfaces;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using Client.Models;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
namespace Client.Services
{
    public class AntiForgeryService : IAntiForgeryService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private string _antiForgeryToken;

        public AntiForgeryService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task<string> FetchTokenAsync()
        {
            var tokenRequest = new HttpRequestMessage(HttpMethod.Get, "api/account/token");
            tokenRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);// nece radit bez Credentials.include

            var tokenResponse = await _httpClient.SendAsync(tokenRequest);

            if (tokenResponse.IsSuccessStatusCode)
            {
                var tokenResponseData = await tokenResponse.Content.ReadFromJsonAsync<AntiForgeryTokenResponse>();
                _antiForgeryToken = tokenResponseData?.Token ?? throw new Exception("Nema token u responsu");

                return _antiForgeryToken;
            }

            throw new Exception($"Failed to fetch anti-forgery token. Status code: {tokenResponse.StatusCode}");
        }
        public async Task<bool> IsTokenValidAsync()
        {
            var cookie = await _jsRuntime.InvokeAsync<string>("getCookie", "XSRF-TOKEN");
            return !string.IsNullOrEmpty(cookie);
        }
        public async Task<string> GetTokenAsync()
        {
            if (await IsTokenValidAsync())
            {
                return _antiForgeryToken ??= await _jsRuntime.InvokeAsync<string>("getCookie", "XSRF-TOKEN");
            }
            return await FetchTokenAsync();
        }
    }
}
