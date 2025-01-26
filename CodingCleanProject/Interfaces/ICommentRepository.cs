using CodingCleanProject.Dtos.Comment;
using CodingCleanProject.Models;

namespace CodingCleanProject.Interfaces
{
    public interface ICommentRepository
    {
        Task<List<Comment>> GetAllCommentAsync();
        Task<Comment?> GetCommentByIdAsync(int id);
        Task<Comment> CreateCommentAsync(Comment comment);
        Task<Comment?> UpdateAsync(UpdateCommentDTO comment, int Id);
        Task<Comment?> DeleteAsync(int Id);
    }
}
