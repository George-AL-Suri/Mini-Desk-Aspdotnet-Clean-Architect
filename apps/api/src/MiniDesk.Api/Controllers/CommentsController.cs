using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniDesk.Application.Comments;
using MiniDesk.Application.Comments.Dtos;

[ApiController]
[Route("api/comments")]
[Authorize]
public class CommentsController(ICommentService service) : ControllerBase
{
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CommentDto>> Update(int id, UpdateCommentRequest request, CancellationToken ct) =>
        Ok(await service.UpdateAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }
}