using ExamApp.Api.Application.DTOs;

namespace ExamApp.Api.Application.Contracts;

public interface IQuestionService
{
    Task<IReadOnlyList<ExamQuestionDto>> GetExamQuestionsAsync(
        string? category,
        string? type,
        CancellationToken ct);

    Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken ct);
}