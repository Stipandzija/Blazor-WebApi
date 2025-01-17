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


        public AuthService(CustomAuthenticationStateProvider customAuthenticationStateProvider, HttpClient httpClient, ILocalStorageService localStorage, IJSRuntime jsRuntime)
        {
            Console.WriteLine("AuthService created");
            _httpClient = httpClient;
            _localStorage = localStorage;
            _jsRuntime = jsRuntime;
            _authenticationStateProvider = customAuthenticationStateProvider;
        }
        public async Task<bool> LoginAsync(string username, string password)
        {
            var request = new { UserName = username, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/account/login", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result != null)
                {
                    await SetTokenAsync(result.JwtToken);

                    _authenticationStateProvider.MarkUserAsAuthenticated(result.JwtToken);
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.JwtToken);
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
                Console.WriteLine("Registration successful");
                return true;
            }
            else
            {
                Console.WriteLine($"Registration failed: {response.StatusCode}");
                return false;
            }
        }

        public async Task<bool> LogoutAsync()
        {
            _authenticationStateProvider.MarkUserAsLoggedOut();
            await _localStorage.RemoveItemAsync("authToken");
            var response = await _httpClient.PostAsJsonAsync("api/account/logout","");

            _httpClient.DefaultRequestHeaders.Authorization = null;


            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Logout successful.");
                return true;
            }
            return false;
        }

        public async Task SetTokenAsync(string token)
        {
            if (token == null)
            {
                await _localStorage.RemoveItemAsync("authToken");
            }
            else
            {
                await _localStorage.SetItemAsync("authToken",token);

            }
        }

        public async Task<bool> RefreshAccessTokenAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync("api/account/RefreshToken", null);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                    if (result != null && result.IsLogged)
                    {
                        await _localStorage.SetItemAsync("authToken", result.JwtToken);
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.JwtToken);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error refreshing token: " + ex.Message);
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
                var success = await RefreshAccessTokenAsync();
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


    public class LoginResponse
    {
        public bool IsLogged { get; set; } = false;
        public string JwtToken { get; set; }
        public string RefreshToken { get; internal set; }

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
