using CodingCleanProject.Dtos.Comment;
using CodingCleanProject.Models;

namespace CodingCleanProject.Mapper
{
    public interface ICommentMapper
    {
        CommentDto ToCommentDto(Comment comment);
        Comment FromCreateDto(CreateCommentDTO commentDto, int stockId);
    }
}
