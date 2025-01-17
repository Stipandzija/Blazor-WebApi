using CodingCleanProject.Models;

namespace CodingCleanProject.Interfaces
{
    public interface IRegisterRepository
    {
        Task<bool> IsUserNameTakenAsync(string userName);
        Task<bool> IsEmailTakenAsync(string email);
        Task<(bool Succeeded, IEnumerable<string> Errors)> CreateUserAsync(User user, string password);
        Task<(bool Succeeded, IEnumerable<string> Errors)> AssignRoleToUserAsync(User user, string role);
    }
}
