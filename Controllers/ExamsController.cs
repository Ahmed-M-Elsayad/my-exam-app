using ExamApp.Api.Application.Contracts;
using ExamApp.Api.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ExamApp.Api.Controllers;

[ApiController]
[Route("api/exams")]
[EnableRateLimiting("exam-submit")]
public class ExamsController : ControllerBase
{
    private readonly IExamService _examService;

    public ExamsController(IExamService examService)
    {
        _examService = examService;
    }

    [HttpPost("evaluate")]
    public async Task<IActionResult> Evaluate(
        [FromBody] ExamSubmissionDto submission,
        CancellationToken ct)
    {
        var result = await _examService.SubmitAsync(submission, ct);
        return Ok(result);
    }
}