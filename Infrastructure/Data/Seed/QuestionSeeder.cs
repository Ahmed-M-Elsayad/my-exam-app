using System.Text.Json;
using System.Text.Json.Serialization;
using ExamApp.Api.Domain.Entities;
using ExamApp.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ExamApp.Api.Infrastructure.Data.Seed;

public static class QuestionSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    public static async Task SeedAsync(ExamDbContext context, string jsonFilePath, ILogger logger)
    {
        if (await context.Questions.AnyAsync())
        {
            logger.LogInformation("Questions already seeded. Skipping.");
            return;
        }

        if (!File.Exists(jsonFilePath))
        {
            logger.LogWarning("Seed file not found at {Path}", jsonFilePath);
            return;
        }

        var json = await File.ReadAllTextAsync(jsonFilePath);
        var payload = JsonSerializer.Deserialize<QuestionBankFile>(json, JsonOptions);

        if (payload?.Questions is null || payload.Questions.Count == 0)
        {
            logger.LogWarning("Seed file is empty or invalid.");
            return;
        }

        foreach (var item in payload.Questions)
            Validate(item);

        var duplicate = payload.Questions
            .GroupBy(q => q.Id)
            .FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null)
            throw new InvalidOperationException($"Duplicate question id {duplicate.Key} in the seed file.");

        var questions = payload.Questions.Select(ToEntity).ToList();
        await context.Questions.AddRangeAsync(questions);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} questions.", questions.Count);
    }

    // بنوقف التشغيل برسالة واضحة بدل ما سؤال غلط يتسجل ويتصحح غلط بعدين
    private static void Validate(QuestionFileItem item)
    {
        if (item.Type is not ("mcq" or "tf"))
            return;

        var optionCount = item.Options?.Count ?? 0;
        if (item.Correct is null || item.Correct < 0 || item.Correct >= optionCount)
        {
            throw new InvalidOperationException(
                $"Question {item.Id}: 'correct' must be the index of one of the options.");
        }
    }

    private static Question ToEntity(QuestionFileItem item) => new()
    {
        Id = item.Id,
        Category = item.Category,
        Type = MapType(item.Type),
        Text = item.Question,
        Options = item.Options ?? new List<string>(),
        CorrectOptionIndex = MapCorrect(item),
        Explanation = item.Explanation,
        ModelAnswer = item.Answer,
        Rubric = item.Rubric,
    };

    private static QuestionType MapType(string type) => type switch
    {
        "mcq" => QuestionType.Mcq,
        "tf" => QuestionType.TrueFalse,
        "essay" => QuestionType.Essay,
        "report" => QuestionType.Report,
        _ => throw new InvalidOperationException($"Unknown question type: {type}"),
    };

    private static int? MapCorrect(QuestionFileItem item) =>
        item.Type is "mcq" or "tf" ? item.Correct : null;

    // شكل ملف questions.json

    private sealed class QuestionBankFile
    {
        public List<QuestionFileItem> Questions { get; set; } = new();
    }

    private sealed class QuestionFileItem
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public List<string>? Options { get; set; }
        public int? Correct { get; set; }
        public string? Explanation { get; set; }
        public string? Answer { get; set; }
        public string? Rubric { get; set; }
    }
}
