using CodingCleanProject.Dtos.Comment;
using CodingCleanProject.Models;

namespace CodingCleanProject.Mapper
{
    public class CommentMapper : ICommentMapper
    {
        public CommentDto ToCommentDto(Comment comment)
        {
            return new CommentDto
            {
                Id = comment.Id,
                Title = comment.Title,
                Content = comment.Content,
                CreateOn = comment.CreateOn,
                StockId = comment.StockId,
                CreatedBy = comment.AppUser.UserName

            };
        }
        public Comment FromCreateDto(CreateCommentDTO comment,int Id)
        {
            return new Comment
            {
                Title = comment.Title,
                Content = comment.Content,
                StockId = Id
            };
        }

    }
}
