namespace Shared.Dtos.Account
{
    public class LoginResponse
    {
        public bool IsLogged { get; set; } = false;
        public string JwtToken { get; set; }
        public string RefreshToken { get; private set; }
        public void SetRefreshToken(string refreshToken)
        {
            RefreshToken = refreshToken;
        }
    }
}
