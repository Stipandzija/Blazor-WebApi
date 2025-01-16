using Blazored.LocalStorage;
using Client.Pages;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Client.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public UserService(HttpClient httpClient, ILocalStorageService localStorageService)
        {
            _httpClient = httpClient;
            _localStorage = localStorageService;
        }

        public async Task<string?> GetTokenTimeLeftAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");

                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("Token nije pronađen.");
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("api/test/Dozvola");

                if (response.IsSuccessStatusCode)
                {
                    return "Imas dopustenje";
                }

                Console.WriteLine($"Greška u dohvaćanju vremena tokena: {response.StatusCode}");
                return $"Error: {response.StatusCode}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri dohvaćanju vremena tokena: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        public class ClaimsResponseModel
        {
            public string UserId { get; set; } = string.Empty;
            public List<ClaimDetails> Claims { get; set; } = new();
        }

        public class ClaimDetails
        {
            public string Type { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
        }
    }
}
