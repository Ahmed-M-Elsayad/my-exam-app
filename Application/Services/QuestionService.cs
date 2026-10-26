using ExamApp.Api.Application.Contracts;
using ExamApp.Api.Application.DTOs;
using ExamApp.Api.Domain.Enums;
using ExamApp.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExamApp.Api.Application.Services;

public class QuestionService : IQuestionService
{
    private readonly ExamDbContext _db;

    public QuestionService(ExamDbContext db) => _db = db;

    public async Task<IReadOnlyList<ExamQuestionDto>> GetExamQuestionsAsync(
        string? category,
        string? type,
        CancellationToken ct)
    {
        var query = _db.Questions.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(category) && category != "all")
            query = query.Where(q => q.Category == category);

        if (!string.IsNullOrWhiteSpace(type) && type != "all" && TryParseType(type, out var parsedType))
            query = query.Where(q => q.Type == parsedType);

        return await query
            .OrderBy(q => q.Id)
            .Select(q => new ExamQuestionDto(
                q.Id,
                q.Category,
                TypeToString(q.Type),
                q.Text,
                q.Options.Count > 0 ? q.Options : null))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken ct)
    {
        return await _db.Questions
            .AsNoTracking()
            .Select(q => q.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(ct);
    }

    private static string TypeToString(QuestionType type) => type switch
    {
        QuestionType.Mcq => "mcq",
        QuestionType.TrueFalse => "tf",
        QuestionType.Essay => "essay",
        QuestionType.Report => "report",
        _ => "unknown",
    };

    private static bool TryParseType(string type, out QuestionType parsedType)
    {
        parsedType = type switch
        {
            "mcq" => QuestionType.Mcq,
            "tf" => QuestionType.TrueFalse,
            "essay" => QuestionType.Essay,
            "report" => QuestionType.Report,
            _ => QuestionType.Mcq,
        };
        return type is "mcq" or "tf" or "essay" or "report";
    }
}