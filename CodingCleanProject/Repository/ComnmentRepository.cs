using CodingCleanProject.Data;
using CodingCleanProject.Dtos.Comment;
using CodingCleanProject.Interfaces;
using CodingCleanProject.Models;
using Microsoft.EntityFrameworkCore;

namespace CodingCleanProject.Repository
{
    public class ComnmentRepository : ICommentRepository
    {
        private readonly AppDbContext _context;
        public ComnmentRepository(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task<Comment> CreateCommentAsync(Comment comment)
        {
            await _context.AddAsync(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment?> DeleteAsync(int Id)
        {

            var CommentModel = await _context.comments.FirstOrDefaultAsync(X=>X.Id==Id);
            if (CommentModel != null)
            {
                _context.comments.Remove(CommentModel);
                await _context.SaveChangesAsync();
                return CommentModel;
            }
            return null;
        }

        public async Task<List<Comment>> GetAllCommentAsync()
        {
            return await _context.comments.Include(x=>x.AppUser).ToListAsync();
        }

        public async Task<Comment?> GetCommentByIdAsync(int id)
        {
            return await _context.comments.Include(x => x.AppUser).FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Comment?> UpdateAsync(UpdateCommentDTO comment,int Id)
        {
            var commentModel = await _context.comments.FirstOrDefaultAsync(X => X.Id == Id);
            if (commentModel != null)
            {
                commentModel.Title = comment.Title;
                commentModel.Content = comment.Content;
                await _context.SaveChangesAsync();
                return commentModel;
            }
            return null;
        }
    }
}
