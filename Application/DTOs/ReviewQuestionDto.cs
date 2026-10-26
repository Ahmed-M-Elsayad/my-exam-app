namespace ExamApp.Api.Application.DTOs;

/// <summary>
/// السؤال بعد التسليم - يحتوي على الإجابة والشرح.
/// </summary>
public record ReviewQuestionDto(
    int QuestionId,
    string Question,
    string? UserChoice,
    string? CorrectAnswer,
    string? Explanation,
    bool IsCorrect);