using Shared.Dtos.Comment;
using CodingCleanProject.Helpers;
using CodingCleanProject.Interfaces;
using CodingCleanProject.Mapper;
using Microsoft.AspNetCore.Mvc;

namespace CodingCleanProject.Controllers
{
    [Route("api/comment")]
    [ApiController]
    public class AppCommentController : Controller
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IStockRepository _stockRepository;
        public AppCommentController(ICommentRepository commentRepository, IStockRepository stockRepository)
        {
            _commentRepository = commentRepository;
            _stockRepository = stockRepository;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllComments() {

            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var CommentModel = await _commentRepository.GetAllCommentAsync();
            return Ok(CommentModel.Select(CommentMapper.ToCommentDto));
        }
        [HttpGet]
        [Route("{Id:int}")]
        public async Task<IActionResult> GetCommentById([FromRoute] int Id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var CommentModel = await _commentRepository.GetCommentByIdAsync(Id);
            if (CommentModel == null)
                return NotFound();
            return Ok(CommentModel.ToCommentDto());
        }
        [HttpPost]
        [Route("{Id:int}")]
        public async Task<IActionResult> CreateComment([FromRoute] int Id, [FromBody] CreateCommentDTO commentDto) 
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _stockRepository.ExistStock(Id) == false)
                return BadRequest("Stock ne postoji");
            var CommentModel = commentDto.CreateCommentToDto(Id);
            await _commentRepository.CreateCommentAsync(CommentModel);
            return CreatedAtAction(nameof(GetCommentById), new { id = CommentModel.Id }, CommentModel.ToCommentDto());
        }
        [HttpPut]
        [Route("{Id:int}")]
        public async Task<IActionResult> UpdateComment([FromRoute] int Id, [FromBody] UpdateCommentDTO UpdateCommentDTO) 
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var CommentModel = await _commentRepository.UpdateAsync(Id, UpdateCommentDTO);
            if (CommentModel == null) return NotFound();
            return Ok(CommentModel.ToCommentDto());
        }
        [HttpDelete]
        [Route("{Id:int}")]
        public async Task<IActionResult> DeleteComment([FromRoute] int Id) 
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var CommentModel = await _commentRepository.DeleteAsync(Id);
            if (CommentModel == null) 
                return NotFound();

            return NoContent();
        }
    }
}
