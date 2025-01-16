namespace CodingCleanProject.Dtos.Account
{
    public class LoginResponse
    {
        public bool IsLogged { get; set; } = false;
        public string JwtToken { get; set; }
        public string RefreshToken { get; internal set; }

    }
}
