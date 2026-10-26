using ExamApp.Api.Domain.Enums;

namespace ExamApp.Api.Domain.Entities;

public class Question
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public string Text { get; set; } = string.Empty;

    // Options بتتخزن في الداتابيز كـ JSON column
    public List<string> Options { get; set; } = new();

    // ممكن تكون null لو النوع Essay أو Report
    public int? CorrectOptionIndex { get; set; }

    public string? Explanation { get; set; }
    public string? ModelAnswer { get; set; }
    public string? Rubric { get; set; }
}