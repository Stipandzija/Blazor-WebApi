using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;

namespace BlazorClient.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly IJSRuntime _jsRuntime;

        public AuthService(HttpClient httpClient, ILocalStorageService localStorage, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _jsRuntime = jsRuntime;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var request = new { UserName = username, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/account/login", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TokenResponse>();

                if (result != null)
                {
                    await _localStorage.SetItemAsync("jwtToken", result.Token);
                    await _jsRuntime.InvokeVoidAsync("setCookie", "RefreshToken", result.RefreshToken, 7);
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);

                    return true;
                }
            }

            return false;
        }



        public async Task<string> GetTokenAsync()
        {
            return await _localStorage.GetItemAsync<string>("jwtToken");
        }

        public async Task<bool> RefreshTokenAsync()
        {
            var response = await _httpClient.PostAsync("api/account/refresh", null);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
                if (result != null)
                {
                    await _localStorage.SetItemAsync("jwtToken", result.Token);
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);

                    return true;
                }
            }

            return false;
        }
        public async Task<HttpResponseMessage> MakeAuthorizedRequest(HttpRequestMessage request)
        {
            var token = await _localStorage.GetItemAsync<string>("jwtToken");

            if (string.IsNullOrEmpty(token))
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // u slucaju erroa novi token
                var success = await RefreshTokenAsync();
                if (success)
                {
                    // potrebno poslat ponovo zahtjev za tokenom
                    token = await _localStorage.GetItemAsync<string>("jwtToken");
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    response = await _httpClient.SendAsync(request);
                }
            }

            return response;
        }

    }

    public class TokenResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
    }
}
