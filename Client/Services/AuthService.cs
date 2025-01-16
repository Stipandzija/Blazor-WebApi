using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;



namespace BlazorClient.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly CustomAuthenticationStateProvider _authenticationStateProvider;
        private readonly IJSRuntime _jsRuntime;

        public AuthService(CustomAuthenticationStateProvider authenticationStateProvider,HttpClient httpClient, ILocalStorageService localStorage, IJSRuntime jsRuntime)
        {
            Console.WriteLine("AuthService created.");
            _httpClient = httpClient;
            _localStorage = localStorage;
            _jsRuntime = jsRuntime;
            _authenticationStateProvider = authenticationStateProvider;
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
                    await SetTokenAsync(result.Token, result.Expires);
                    _authenticationStateProvider.MarkUserAsAuthenticated(result.Token);
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
                    return true;
                }
            }
            else
            {
                Console.WriteLine($"Login failed: {response.StatusCode}");
            }

            return false;
        }
        public async Task<bool> RegisterAsync(string username, string email, string password)
        {
            var request = new { UserName = username, EmailAddress = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/account/register", request);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Registration successful.");
                return true;
            }
            else
            {
                Console.WriteLine($"Registration failed: {response.StatusCode}");
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync("authToken");
            _authenticationStateProvider.MarkUserAsLoggedOut(); 
            await _jsRuntime.InvokeVoidAsync("deleteCookie", "RefreshToken");

            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task SetTokenAsync(string token, DateTime expiry)
        {
            if (token == null)
            {
                await _localStorage.RemoveItemAsync("authToken");
                await _localStorage.RemoveItemAsync("authTokenExpires");
            }
            else
            {
                await _localStorage.SetItemAsync("authToken", token);
                await _localStorage.SetItemAsync("authTokenExpires", expiry.ToString("o"));
            }
        }

        //public async Task<string> GetTokenAsync()
        //{
        //    var expiry = await _localStorage.GetItemAsync<string>("authTokenExpiry");
        //    if (expiry != null)
        //    {
        //        if (DateTime.Parse(expiry.ToString()) > DateTime.Now)
        //        {
        //            return await _localStorage.GetItemAsync<string>("authToken");
        //        }
        //        else
        //        {
        //            await SetTokenAsync(null);
        //        }
        //    }
        //    return null;
        //}


        public async Task<bool> RefreshTokenAsync()
        {
            var response = await _httpClient.PostAsync("api/account/refresh", null);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
                if (result != null)
                {
                    await _localStorage.SetItemAsync("authToken", result.Token);
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);

                    return true;
                }
            }

            return false;
        }
        public async Task<HttpResponseMessage> MakeAuthorizedRequest(HttpRequestMessage request)
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");

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
                    token = await _localStorage.GetItemAsync<string>("authToken");
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
        public DateTime Expires { get; set; }
    }
    public class RegisterDTO
    {
        [Required(ErrorMessage = "User name is required")]
        public string userName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string emailAddress { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        public string password { get; set; }
    }
}
