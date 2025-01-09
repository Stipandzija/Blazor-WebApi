using CodingCleanProject.Models;

namespace CodingCleanProject.Interfaces
{
    public interface IRefreshToken
    {
        RefreshToken GenerateRefreshToken();
    }
}
