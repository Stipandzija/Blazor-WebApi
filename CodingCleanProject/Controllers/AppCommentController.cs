using CodingCleanProject.Interfaces;
using CodingCleanProject.Mapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CodingCleanProject.Models;
using CodingCleanProject.Extensions.ClaimsExtenions;
using CodingCleanProject.Dtos.Comment;

namespace CodingCleanProject.Controllers
{
    [Route("api/comment")]
    [ApiController]
    public class AppCommentController : Controller
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IStockRepository _stockRepository;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public AppCommentController(UserManager<User> userManager, ICommentRepository commentRepository, IStockRepository stockRepository, IMapper mapper)
        {
            _commentRepository = commentRepository;
            _stockRepository = stockRepository;
            _userManager = userManager;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllComments() {

            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var CommentModel = await _commentRepository.GetAllCommentAsync();
            var commentDtos = CommentModel.Select(comment => _mapper.CommentMapper.ToCommentDto(comment));
            return Ok(commentDtos);
        }
        [HttpGet]
        [Route("{Id:int}")]
        public async Task<IActionResult> GetCommentById([FromRoute] int Id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var commentModels = await _commentRepository.GetAllCommentAsync();
            var commentDtos = commentModels.Select(comment => _mapper.CommentMapper.ToCommentDto(comment)); // Using ICommentMapper
            return Ok(commentDtos);
        }
        [HttpPost]
        [Route("{Id:int}")]
        public async Task<IActionResult> CreateComment([FromRoute] int Id, [FromBody] CreateCommentDTO commentDto) 
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _stockRepository.ExistStock(Id) == false)
                return BadRequest("Stock ne postoji");

            var UserName = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(UserName);

            var commentModel = _mapper.CommentMapper.FromCreateDto(commentDto,Id);
            commentModel.AppUserId = appUser.Id;
            commentModel.StockId = Id;

            await _commentRepository.CreateCommentAsync(commentModel);

            var commentDtoResult = _mapper.CommentMapper.ToCommentDto(commentModel);
            return CreatedAtAction(nameof(GetCommentById), new { id = commentModel.Id }, commentDtoResult);
        }
        [HttpPut]
        [Route("{Id:int}")]
        public async Task<IActionResult> UpdateComment([FromRoute] int Id, [FromBody] UpdateCommentDTO UpdateCommentDTO) 
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var CommentModel = await _commentRepository.UpdateAsync(UpdateCommentDTO,Id);
            if (CommentModel == null) return NotFound();

            var commentDto = _mapper.CommentMapper.ToCommentDto(CommentModel);
            return Ok(commentDto);
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
