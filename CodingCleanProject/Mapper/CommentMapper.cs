using Shared.Dtos.Comment;
using CodingCleanProject.Models;
using CodingCleanProject.Repository;

namespace CodingCleanProject.Mapper
{
    public static class CommentMapper
    {
        public static CommentDto ToCommentDto(this Comment comment)
        {
            return new CommentDto
            {
                Title = comment.Title,
                Content = comment.Content,
                CreateOn = comment.CreateOn,
                StockId = comment.StockId,
            };
        }
        public static Comment CreateCommentToDto(this CreateCommentDTO comment,int Id)
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
