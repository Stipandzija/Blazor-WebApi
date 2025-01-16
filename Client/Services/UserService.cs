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

        public async Task<UserDetailsModel?> GetUserDetailsAsync()
        {
            try
            {

                var token = await _localStorage.GetItemAsync<string>("authToken");
                Console.WriteLine(token.ToString());
                
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
               
                return await _httpClient.GetFromJsonAsync<UserDetailsModel>("api/account/getUserDetails");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

    }

    public class UserDetailsModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public List<ClaimDetails> Claims { get; set; }
    }

    public class ClaimDetails
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
