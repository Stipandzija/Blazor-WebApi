namespace Client.Interfaces
{
    public interface IAntiForgeryService
    {
        Task<bool> IsTokenValidAsync();
        Task<string> GetTokenAsync();
        Task<string> FetchTokenAsync();
    }
}
