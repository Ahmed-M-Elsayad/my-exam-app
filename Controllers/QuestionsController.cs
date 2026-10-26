using ExamApp.Api.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ExamApp.Api.Controllers;

[ApiController]
[Route("api/questions")]
[EnableRateLimiting("questions-read")]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;

    public QuestionsController(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string? category,
        [FromQuery] string? type,
        CancellationToken ct)
    {
        var questions = await _questionService.GetExamQuestionsAsync(category, type, ct);
        return Ok(new { total = questions.Count, questions });
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var categories = await _questionService.GetCategoriesAsync(ct);
        return Ok(categories);
    }
}